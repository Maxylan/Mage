using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authentication;
using ReceptionAuthorizationService = Reception.Interfaces.IAuthorizationService;
using Reception.Database;
using Reception.Database.Models;
using Reception.Interfaces;

namespace Reception.Middleware.Authentication;

/// <summary>
/// Custom implementation of the opinionated <see cref="IAuthenticationHandler"/> '<see cref="AuthenticationHandler{AuthenticationSchemeOptions}"/>'.
/// Intercepts incoming requests and checks if a valid session token is provided with the request.
/// </summary>
public class MemoAuth(
    ILoggingService<MemoAuth> logging,
    ReceptionAuthorizationService service,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>
    /// Core validation logic for our custom authentication schema.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        bool getSessionTokenHeader = Request.Headers.TryGetValue(
            Constants.SESSION_TOKEN_HEADER,
            out StringValues headerValue
        );

        if (!getSessionTokenHeader)
        {
            return AuthenticateResult.Fail(Messages.MissingHeader);
        }

        var token = headerValue.ToString();
        var getSession = await service.ValidateSession(token, Source.EXTERNAL);
        Session? session = getSession.Value;

        if (session is null || string.IsNullOrWhiteSpace(session.Code))
        {
            string message = $"{Messages.ValidationFailed} Token: '{token}', Result '{getSession.GetType().FullName}', Session: {(session?.GetType()?.Name ?? "null")}).";
            logging
                .Action(nameof(HandleAuthenticateAsync))
                .ExternalWarning(message)
                .LogAndEnqueue();

            return AuthenticateResult.Fail(
                Program.IsProduction ? Messages.ValidationFailed : message
            );
        }

        AuthenticationTicket? ticket = null;
        try
        {
            ticket = GenerateAuthenticationTicket(session!.Account, session);
        }
        catch (AuthenticationException authException)
        {
            string message = Messages.ByCode(authException);
            logging
                .Action(nameof(HandleAuthenticateAsync))
                .LogInformation(message, opts =>
                {
                    opts.Exception = authException;
                    opts.Source = Source.EXTERNAL;
                    opts.LogLevel = authException.Code switch
                    {
                        1 => Severity.ERROR,
                        2 => Severity.SUSPICIOUS,
                        _ => Severity.INFORMATION
                    };
                })
                .LogAndEnqueue();

            return AuthenticateResult.Fail(message);
        }
        catch (Exception ex)
        {
            string message = Messages.UnknownError + " " + ex.Message;
            logging
                .Action(nameof(HandleAuthenticateAsync))
                .ExternalError(message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return AuthenticateResult.Fail(
                Program.IsProduction ? Messages.UnknownError : message
            );
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
            AuthenticationException.Throw(Messages.UnknownErrorCode);
            /* Logger.LogInformation($"[{nameof(MemoAuth)}] ({nameof(GenerateAuthenticationTicket)}) Loading missing navigation entries.");

            foreach (var navigationEntry in db.Entry(user).Navigations)
            {
                navigationEntry.Load();
            }

            if (user.Sessions is null || user.Sessions.Count == 0)
            {
                AuthenticationException.Throw(Messages.UnknownErrorCode);
            } */
        }

        if (session.Client is null)
        {
            AuthenticationException.Throw(Messages.UnknownErrorCode);
            /* ... */
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
                { Constants.TOKEN_CONTEXT_KEY, session.Code },
            },
            new Dictionary<string, object?>() {
                { Constants.ACCOUNT_CONTEXT_KEY, user },
                { Constants.SESSION_CONTEXT_KEY, session },
                { Constants.CLIENT_CONTEXT_KEY, session.Client }
            }
        ) {

        };

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
            remoteAddress.AddressFamily == AddressFamily.InterNetwork
        )
        {
            return remoteAddress.ToString();
        }

        string? remoteAddressValue = null;
        bool hasForwardedForHeader = context.Request.Headers.ContainsKey("X-Forwarded-For");
        if (hasForwardedForHeader) // HTTP_X_FORWARDED_FOR
        {
            remoteAddressValue = context.Request.Headers["X-Forwarded-For"].ToString();
        }

        if (string.IsNullOrWhiteSpace(remoteAddressValue))
        {
            bool hasRemoteAddrHeader = context.Request.Headers.ContainsKey("Remote-Addr");

            if (hasRemoteAddrHeader)
            { // REMOTE_ADDR
                remoteAddressValue = context.Request.Headers["Remote-Addr"].ToString();
            }

            if (string.IsNullOrWhiteSpace(remoteAddressValue))
            {
                return null;
            }
        }

        if (Program.IsDevelopment)
        {
            Console.WriteLine($"[{nameof(MemoAuth)}] (Debug) {nameof(GetRemoteAddress)} -> Returning a remote-address header. ({remoteAddressValue})");
        }

        return remoteAddressValue.Split(',')[^1].Trim();
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

        if (authentication is null || !authentication.Succeeded)
        {
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

        if (authentication is null)
        {
            AuthenticationException.Throw(Messages.MissingAuthorizationResultCode);
        }
        else if (!authentication.Succeeded)
        {
            AuthenticationException.Throw(Messages.UnauthorizedCode);
        }

        return authentication!.Properties!;
    }

    /// <summary>
    /// Attempt to get the <see cref="AuthenticationProperties"/> associated with this request.
    /// </summary>
    /// <remarks>
    /// Unlike <seealso cref="MemoAuth.Properties"/>, this returns a <c>bool</c> flagging success instead of throwing when missing.
    /// </remarks>
    public static bool TryGetProperties(IHttpContextAccessor contextAccessor, [NotNullWhen(true)] out AuthenticationProperties? properties) =>
        TryGetProperties(contextAccessor.HttpContext!, out properties);
    /// <summary>
    /// Attempt to get the <see cref="AuthenticationProperties"/> associated with this request.
    /// </summary>
    /// <remarks>
    /// Unlike <seealso cref="MemoAuth.Properties"/>, this returns a <c>bool</c> flagging success instead of throwing when missing.
    /// </remarks>
    public static bool TryGetProperties(HttpContext context, [NotNullWhen(true)] out AuthenticationProperties? properties)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        properties = null;
        var getAuthenticationResult = context.Features.Get<IAuthenticateResultFeature>();
        var authentication = getAuthenticationResult?.AuthenticateResult;

        if (authentication is null || !authentication.Succeeded)
        {
            return false;
        }

        properties = authentication.Properties;
        return true;
    }


    /// <summary>
    /// Attempt to get the current user's <see cref="Account"/> from the '<see cref="AuthenticationProperties"/>'
    /// </summary>
    /// <remarks>
    /// Unlike <seealso cref="MemoAuth.GetAccount"/>, this returns a <c>bool</c> flagging success instead of throwing when missing.
    /// </remarks>
    public static bool TryGetAccount(IHttpContextAccessor contextAccessor, [NotNullWhen(true)] out Account? account) =>
        TryGetAccount(contextAccessor.HttpContext!, out account);
    /// <summary>
    /// Attempt to get the current user's <see cref="Account"/> from the '<see cref="AuthenticationProperties"/>'
    /// </summary>
    /// <remarks>
    /// Unlike <seealso cref="MemoAuth.GetAccount"/>, this returns a <c>bool</c> flagging success instead of throwing when missing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static bool TryGetAccount(HttpContext httpContext, [NotNullWhen(true)] out Account? account)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        account = null;

        if (MemoAuth.TryGetProperties(httpContext, out var properties))
        {
            account = properties.GetParameter<Account>(Constants.ACCOUNT_CONTEXT_KEY);
            return account is not null;
        }

        return false;
    }


    /// <summary>
    /// Get the current user's <see cref="Account"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MemoAuth.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
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
    /// Uses '<see cref="MemoAuth.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static Account? GetAccount(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        return MemoAuth.Properties(httpContext!).GetParameter<Account>(Constants.ACCOUNT_CONTEXT_KEY);
    }


    /// <summary>
    /// Get the current session's <see cref="Client"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MemoAuth.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static Client? GetClient(IHttpContextAccessor contextAccessor) =>
        GetClient(contextAccessor.HttpContext!);
    /// <summary>
    /// Get the current session's <see cref="Client"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MemoAuth.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static Client? GetClient(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        return MemoAuth.Properties(httpContext!).GetParameter<Client>(Constants.CLIENT_CONTEXT_KEY);
    }


    /// <summary>
    /// Get the current user's <see cref="Session"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MemoAuth.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
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
    /// Uses '<see cref="MemoAuth.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static Session? GetSession(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        return MemoAuth.Properties(httpContext!).GetParameter<Session>(Constants.SESSION_CONTEXT_KEY);
    }

    /// <summary>
    /// Get the current user's Token (string) from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Uses '<see cref="MemoAuth.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
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
    /// Uses '<see cref="MemoAuth.Properties(HttpContext)"/>', which can throw a few different errors, depending on failure.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If provided '<see cref="HttpContext"/>' is null
    /// </exception>
    public static string? GetToken(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        return MemoAuth.Properties(httpContext!).Items[Constants.TOKEN_CONTEXT_KEY];
    }
}
