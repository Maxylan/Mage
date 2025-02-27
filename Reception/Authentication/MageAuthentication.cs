using ReceptionAuthorizationService = Reception.Interfaces.IAuthorizationService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Reception.Services;
using System.Security.Claims;
using Reception.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

namespace Reception.Authentication;

/// <summary>
/// Custom implementation of the opinionated <see cref="IAuthenticationHandler"/> '<see cref="AuthenticationHandler{AuthenticationSchemeOptions}"/>'.
/// Intercepts incoming requests and checks if a valid session token is provided with the request.
/// </summary>
public class MageAuthentication(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    MageDbContext db,
    ReceptionAuthorizationService service
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>
    /// Core validation logic for our custom authentication schema.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        bool getSessionTokenHeader = Request.Headers.TryGetValue(
            Parameters.SESSION_TOKEN_HEADER,
            out StringValues headerValue
        );
        
        if (!getSessionTokenHeader) {
            return AuthenticateResult.Fail(Messages.MissingHeader);
        }

        var token = headerValue.ToString();
        var getSession = await service.ValidateSession(token);
        Session? session = getSession.Value;
        
        if (session is null || getSession.Result is not OkObjectResult)
        {
            Logger.LogWarning($"Validation of session '{token}' failed with '{getSession.Result!.GetType().FullName}'");
            return AuthenticateResult.Fail(Messages.MissingHeader);
        }

        AuthenticationTicket? ticket = null;
        try {
            ticket = GenerateAuthenticationTicket(session.User, session);
        }
        catch (AuthenticationException authException) {
            return AuthenticateResult.Fail(Messages.ByCode(authException));
        }
        catch (Exception ex) {
            Logger.LogError(ex, Messages.UnknownError + " " + ex.Message, ex.StackTrace);
            return AuthenticateResult.Fail(Messages.UnknownError + (
                Program.IsProduction ? "" : " " + ex.Message
            ));
        }
        
        return AuthenticateResult.Success(ticket!);
    }

    /// <summary>
    /// Like the name suggests; generates an Authentication Ticket when provided with an <see cref="Account"/> that in turn has a valid session.
    /// </summary>
    private AuthenticationTicket GenerateAuthenticationTicket(Account user, Session session)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(session, nameof(session));
        ArgumentException.ThrowIfNullOrWhiteSpace(user.FullName, nameof(Account.FullName));
        ArgumentException.ThrowIfNullOrWhiteSpace(user.Username, nameof(Account.FullName));

        if (user.Sessions is null || user.Sessions.Count == 0)
        {
            Logger.LogInformation($"[{nameof(MageAuthentication)}] ({nameof(GenerateAuthenticationTicket)}) Loading missing navigation entries.");

            foreach(var navigationEntry in db.Entry(user).Navigations) {
                navigationEntry.Load();
            }

            if (user.Sessions is null || user.Sessions.Count == 0) {
                AuthenticationException.Throw(Messages.UnknownErrorCode);
            }
        }

        Claim[] identityClaims = [
            new Claim(ClaimTypes.NameIdentifier, user.Username, ClaimValueTypes.String, ClaimsIssuer),
            new Claim(ClaimTypes.Name, user.FullName, ClaimValueTypes.String, ClaimsIssuer)
        ];

        ClaimsPrincipal principal = new ClaimsPrincipal(
            new ClaimsIdentity(identityClaims, Scheme.Name)
        );

        AuthenticationProperties properties = new(
            new Dictionary<string, string?>() {
                { Parameters.SESSION_CONTEXT_KEY, "a" }
            },
            new Dictionary<string, object?>() {
                { Parameters.ACCOUNT_CONTEXT_KEY, user }
            }
        );

        AuthenticationTicket ticket = new(principal, properties, Scheme.Name);
        return ticket;
    }
    

    // Static Methods

    /// <summary>
    /// Attempt to get the <see cref="IPAddress"/> associated with this request.
    /// </summary>
    /// <remarks>
    /// Tries <seealso cref="ConnectionInfo.RemoteIpAddress"/>, <seealso cref="HttpRequest.Headers"/><c>["HTTP_X_FORWARDED_FOR"]</c> and
    /// <seealso cref="HttpRequest.Headers"/><c>["REMOTE_ADDR"]</c>, in that exact order.
    /// </remarks>
    public static string? GetRemoteAddress(IHttpContextAccessor contextAccessor) =>
        GetRemoteAddress(contextAccessor.HttpContext!);
    /// <summary>
    /// Attempt to get the <see cref="IPAddress"/> associated with this request.
    /// </summary>
    /// <remarks>
    /// Tries <seealso cref="ConnectionInfo.RemoteIpAddress"/>, <seealso cref="HttpRequest.Headers"/><c>["HTTP_X_FORWARDED_FOR"]</c> and
    /// <seealso cref="HttpRequest.Headers"/><c>["REMOTE_ADDR"]</c>, in that exact order.
    /// </remarks>
    public static string? GetRemoteAddress(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        IPAddress? remoteAddress = context.Connection.RemoteIpAddress;
        if (remoteAddress is not null && 
            remoteAddress.AddressFamily is AddressFamily.InterNetwork
        ) {
            if (Program.IsDevelopment) {
                Console.WriteLine($"[{nameof(MageAuthentication)}] (Debug) {nameof(GetRemoteAddress)} -> Returning a stringified 'context.Connection.RemoteIpAddress'.");
            }
            return remoteAddress.ToString();
        }

        string? remoteAddressValue = null;
        bool hasForwardedForHeader = context.Request.Headers.ContainsKey("HTTP_X_FORWARDED_FOR");
        if (hasForwardedForHeader)
        {
            remoteAddressValue = context.Request.Headers["HTTP_X_FORWARDED_FOR"].ToString();
        }

        if (string.IsNullOrWhiteSpace(remoteAddressValue))
        {
            bool hasRemoteAddrHeader = context.Request.Headers.ContainsKey("REMOTE_ADDR");

            if (hasRemoteAddrHeader) {
                remoteAddressValue = context.Request.Headers["REMOTE_ADDR"].ToString();
            }

            if (string.IsNullOrWhiteSpace(remoteAddressValue))
            {
                if (Program.IsDevelopment) {
                    Console.WriteLine($"[{nameof(MageAuthentication)}] (Debug) {nameof(GetRemoteAddress)} -> Failed to find a remote address tied to the current request.");
                }

                return null;
            }
        }

        if (Program.IsDevelopment) {
            Console.WriteLine($"[{nameof(MageAuthentication)}] (Debug) {nameof(GetRemoteAddress)} -> Returning a remote-address header. ({remoteAddressValue})");
        }

        return remoteAddressValue.Split(',')[^0].Trim();
    }


    /// <summary>
    /// Attempts to determine if the current requesting user is authenticated.
    /// </summary>
    public static bool IsAuthenticated(IHttpContextAccessor contextAccessor) =>
        IsAuthenticated(contextAccessor.HttpContext!);
    /// <summary>
    /// Attempts to determine if the current requesting user is authenticated.
    /// </summary>
    public static bool IsAuthenticated(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var getAuthenticationResult = context.Features.Get<IAuthenticateResultFeature>();
        var authentication = getAuthenticationResult?.AuthenticateResult;
        
        if (authentication is null || !authentication.Succeeded) {
            return false;
        }

        return true;
    }


    /// <summary>
    /// Attempt to get the <see cref="AuthenticationProperties"/> associated with this request.
    /// </summary>
    /// <remarks>
    /// Throws <seealso cref="AuthenticationProperties"/> w/ relevant error codes if the attempt was unsuccessful.
    /// </remarks>
    public static AuthenticationProperties Properties(IHttpContextAccessor contextAccessor) =>
        Properties(contextAccessor.HttpContext!);
    /// <summary>
    /// Attempt to get the <see cref="AuthenticationProperties"/> associated with this request.
    /// </summary>
    /// <remarks>
    /// Throws <seealso cref="AuthenticationProperties"/> w/ relevant error codes if the attempt was unsuccessful.
    /// </remarks>
    public static AuthenticationProperties Properties(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var getAuthenticationResult = context.Features.Get<IAuthenticateResultFeature>();
        var authentication = getAuthenticationResult?.AuthenticateResult;
        
        if (authentication is null) {
            AuthenticationException.Throw(Messages.MissingAuthorizationResultCode);
        }
        else if (!authentication.Succeeded) {
            AuthenticationException.Throw(Messages.UnauthorizedCode);
        }

        return authentication!.Properties!;
    }

    /// <summary>
    /// Attempt to get the <see cref="AuthenticationProperties"/> associated with this request.
    /// </summary>
    /// <remarks>
    /// Unlike <seealso cref="MageAuthentication.Properties"/>, this returns a <c>bool</c> flagging success instead of throwing when missing.
    /// </remarks>
    public static bool TryGetProperties(IHttpContextAccessor contextAccessor, out AuthenticationProperties? properties) =>
        TryGetProperties(contextAccessor.HttpContext!, out properties);
    /// <summary>
    /// Attempt to get the <see cref="AuthenticationProperties"/> associated with this request.
    /// </summary>
    /// <remarks>
    /// Unlike <seealso cref="MageAuthentication.Properties"/>, this returns a <c>bool</c> flagging success instead of throwing when missing.
    /// </remarks>
    public static bool TryGetProperties(HttpContext context, out AuthenticationProperties? properties)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        properties = null;
        var getAuthenticationResult = context.Features.Get<IAuthenticateResultFeature>();
        var authentication = getAuthenticationResult?.AuthenticateResult;
        
        if (authentication is null || !authentication.Succeeded) {
            return false;
        }

        properties = authentication.Properties;
        return true;
    }


    /// <summary>
    /// Get the current user's <see cref="Account"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MageAuthentication.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static Account? GetAccount(IHttpContextAccessor contextAccessor) =>
        GetAccount(contextAccessor.HttpContext!);
    /// <summary>
    /// Get the current user's <see cref="Account"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MageAuthentication.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static Account? GetAccount(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        return MageAuthentication.Properties(httpContext!).GetParameter<Account>(Parameters.ACCOUNT_CONTEXT_KEY);
    }


    /// <summary>
    /// Get the current user's <see cref="Session"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MageAuthentication.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static Session? GetSession(IHttpContextAccessor contextAccessor) =>
        GetSession(contextAccessor.HttpContext!);
    /// <summary>
    /// Get the current user's <see cref="Session"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MageAuthentication.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static Session? GetSession(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        return MageAuthentication.Properties(httpContext!).GetParameter<Session>(Parameters.SESSION_CONTEXT_KEY);
    }

    /// <summary>
    /// Get the current user's Token (string) from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MageAuthentication.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static string? GetToken(IHttpContextAccessor contextAccessor) =>
        GetToken(contextAccessor.HttpContext!);
    /// <summary>
    /// Get the current user's Token (string) from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MageAuthentication.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static string? GetToken(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        return MageAuthentication.Properties(httpContext!).Items[Parameters.ACCOUNT_CONTEXT_KEY];
    }
}
