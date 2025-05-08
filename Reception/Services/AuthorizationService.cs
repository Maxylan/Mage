
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Reception.Database.Models;
using Reception.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Utilities;
using Reception.Caching;
using System.Net;
using Reception.Models;

namespace Reception.Services;

public class AuthorizationService(
    IHttpContextAccessor contextAccessor,
    ILoggingService<AuthorizationService> logging,
    LoginTracker loginTracker,
    ISessionService sessions,
    IAccountService accounts
) : IAuthorizationService
{
    /// <summary>
    /// Validates that a session (..inferred from `<see cref="HttpContext"/>`) ..exists and is valid.
    /// </summary>
    /// <remarks>
    /// Argument <paramref name="source"/> Assumes <see cref="Source.EXTERNAL"/> by-default
    /// </remarks>
    /// <param name="source">Assumes <see cref="Source.EXTERNAL"/> by-default</param>
    public async Task<ActionResult<Session>> ValidateSession(Source source = Source.EXTERNAL)
    {
        string message = string.Empty;
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            message = $"{nameof(Session)} Validation Failed: No {nameof(HttpContext)} found.";
            logging
                .LogError(message, m =>
                {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        bool getAuthenticationProperties = MageAuthentication.TryGetProperties(httpContext, out AuthenticationProperties? authenticationProperties);
        if (!getAuthenticationProperties)
        {
            message = $"{nameof(Session)} Validation Failed: No {nameof(AuthenticationProperties)} found.";
            logging
                .LogError(message, m =>
                {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        var user = authenticationProperties!.GetParameter<Account>(Parameters.ACCOUNT_CONTEXT_KEY);
        if (user is not null)
        {
            var getSession = await sessions.GetSessionByUser(user);
            if (getSession.Value is not null)
            {
                return await ValidateSession(
                    getSession.Value,
                    source
                );
            }
        }

        var session = authenticationProperties!.GetParameter<Session>(Parameters.SESSION_CONTEXT_KEY);
        if (session is not null)
        {
            return await ValidateSession(
                session,
                source
            );
        }

        bool tokenExists = authenticationProperties!.Items.TryGetValue(Parameters.TOKEN_CONTEXT_KEY, out string? token);
        if (tokenExists && !string.IsNullOrWhiteSpace(token))
        {
            return await ValidateSession(
                token,
                source
            );
        }

        message = $"Failed to infer a {nameof(Session)} or Token from contextual {nameof(Account)}, {nameof(Session.Code)} or {nameof(AuthenticationProperties)}";
        logging
            .LogInformation(message, m =>
            {
                m.Action = nameof(ValidateSession);
                m.Source = source;
            })
            .LogAndEnqueue();

        return new UnauthorizedObjectResult(message);
    }
    /// <summary>
    /// Validates that a given <see cref="Session.Code"/> (string) is valid.
    /// </summary>
    public async Task<ActionResult<Session>> ValidateSession(string sessionCode, Source source = Source.INTERNAL)
    {
        var getSession = await sessions.GetSession(sessionCode);
        var session = getSession.Value;

        if (session is not null)
        {
            return await ValidateSession(session, source);
        }

        if (getSession.Result is NotFoundObjectResult)
        {
            return new UnauthorizedObjectResult(
                Program.IsDevelopment
                    ? $"Failed to get a {nameof(Session)} from the {nameof(sessionCode)} '{nameof(sessionCode)}'"
                    : HttpStatusCode.Unauthorized.ToString()
            );
        }

        return getSession;
    }
    /// <summary>
    /// Validates that a given <see cref="Session"/> is valid.
    /// </summary>
    public async Task<ActionResult<Session>> ValidateSession(Session session, Source source = Source.INTERNAL)
    {
        string message = string.Empty;
        var httpContext = contextAccessor.HttpContext;

        if (httpContext is null)
        {
            message = $"{nameof(Session)} Validation Failed: No {nameof(HttpContext)} found.";
            logging
                .LogError(message, m =>
                {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            message = $"{nameof(Session)} Validation Failed: Expired";
            logging
                .LogInformation(message, m =>
                {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (session.HasUserAgent)
        {
            if (session.UserAgent != httpContext.Request.Headers.UserAgent.ToString())
            {
                message = $"{nameof(Session)} Validation Failed: UserAgent missmatch";
                logging
                    .LogSuspicious(message, m =>
                    {
                        m.Action = nameof(ValidateSession);
                        m.Source = source;
                    })
                    .LogAndEnqueue();

                return new UnauthorizedObjectResult(
                    Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
                );
            }

            // Check if 24h expiry is valid..
            if (session.ExpiresAt > DateTime.UtcNow + TimeSpan.FromDays(1))
            {
                message = $"{nameof(Session)} Validation Failed: Invalid Expiry";
                logging
                    .LogSuspicious(message, m =>
                    {
                        m.Action = nameof(ValidateSession);
                        m.Source = source;
                    })
                    .LogAndEnqueue();

                return new UnauthorizedObjectResult(
                    Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
                );
            }
        }
        // Check if 1h expiry is valid..
        // (Expiry-time is shortened when UserAgent is omitted, to minimize the damage that can be done by unauthorized access)
        else if (session.ExpiresAt > DateTime.UtcNow + TimeSpan.FromHours(1))
        {
            message = $"{nameof(Session)} Validation Failed: Invalid Expiry";
            logging
                .LogSuspicious(message, m =>
                {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        var getAccount = await sessions.GetUserBySession(session);
        var account = getAccount.Value;

        if (account is null)
        {
            message = $"{nameof(Session)} Validation Failed: Bad Account.";
            logging
                .LogSuspicious(message, m =>
                {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }
        else if (session.User is null)
        {
            session.User = account;
        }

        logging.Logger.LogTrace($"{nameof(Session)} Validation Success");
        return session;
    }

    /// <summary>
    /// Attempt to "login" (..refresh the session) ..of a given <see cref="Account"/> and its hashed password.
    /// </summary>
    /// <param name="userName">Unique Username of an <see cref="Account"/></param>
    /// <param name="hash">SHA-256</param>
    public async Task<ActionResult<Session>> Login(string userName, string hash)
    {
        /* Account? account = await db.Accounts
            .Include(acc => acc.Sessions)
            .FirstOrDefaultAsync(acc => acc.Username == userName); */
        var getAccount = await accounts.GetAccountByUsername(userName);
        Account? account = getAccount.Value;

        if (account is null)
        {
            // To rate-limit password attempts, even in this early fail-fast check..
            Thread.Sleep(512);
            return getAccount.Result!;
        }

        return await Login(account, hash);
    }
    /// <summary>
    /// Attempt to "login" (..refresh the session) ..of a given <see cref="Account"/> and its hashed password.
    /// </summary>
    /// <param name="hash">SHA-256</param>
    public async Task<ActionResult<Session>> Login(Account account, string hash)
    {
        Thread.Sleep(512); // To sortof rate-limit password attempts..

        if (contextAccessor.HttpContext is null)
        {
            string message = $"Login Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(Login))
                .ExternalError(message)
                .LogAndEnqueue();

            return new ObjectResult(
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : message
            )
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        string? userAgent = contextAccessor.HttpContext.Request.Headers.UserAgent.ToString();
        string? userAddress = MageAuthentication.GetRemoteAddress(contextAccessor.HttpContext);

        if (!string.IsNullOrWhiteSpace(userAddress))
        {
            userAgent = userAgent
                .Normalize()
                .Subsmart(0, 255)
                .Replace(@"\", string.Empty)
                .Replace("&&", string.Empty)
                .Trim();
        }
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = userAgent
                .Normalize()
                .Subsmart(0, 1023)
                .Replace(@"\", string.Empty)
                .Replace("&&", string.Empty)
                .Trim();
        }

        if (loginTracker.Attempts(account.Username, userAddress) >= 3)
        {
            string message = $"Failed to login user '{account.Username}' (#{account.Id}). Timeout due to repeatedly failed attempts.";
            logging
                .Action(nameof(Login))
                .ExternalSuspicious(message)
                .LogAndEnqueue();

            return new ObjectResult(
                Program.IsProduction ? HttpStatusCode.RequestTimeout.ToString() : message
            )
            {
                StatusCode = StatusCodes.Status408RequestTimeout
            };
        }

        if (account.Password != hash)
        {
            loginTracker.RecordAttempt(account.Username, userAddress, userAgent);

            string message = $"Failed to login user '{account.Username}' (#{account.Id}). Password Missmatch.";
            logging
                .Action(nameof(Login))
                .ExternalSuspicious(message)
                .LogAndEnqueue();

            await sessions.CleanupSessions();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        var createSession = await sessions.CreateSession(account, contextAccessor.HttpContext.Request, Source.EXTERNAL);
        var session = createSession.Value;

        if (createSession.Result is NoContentResult)
        {
            logging
                .Action(nameof(Login))
                .ExternalTrace($"No new session created ({nameof(NoContentResult)})")
                .LogAndEnqueue();

            var getSession = await sessions.GetSessionByUser(account);
            return getSession;
        }
        else if (session is null)
        {
            string message = $"Failed to login user '{account.Username}' (#{account.Id}). Could not create a new {nameof(Session)} ({nameof(createSession.Result)}).";
            logging
                .Action(nameof(Login))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return createSession.Result!;
        }

        return session;
    }
}
