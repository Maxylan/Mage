
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Reception.Models.Entities;
using Reception.Interfaces;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Reception.Services;

public class AuthorizationService(
    IHttpContextAccessor contextAccessor,
    ILoggingService logging,
    ISessionService sessions,
    MageDbContext db
) : IAuthorizationService
{
    /// <summary>
    /// Validates that a session (..inferred from `<see cref="HttpContext"/>`) ..exists and is valid.
    /// </summary>
    /// <remarks>
    /// Argument <paramref name="source"/> Assumes <see cref="Source.EXTERNAL"/> by-default
    /// </remarks>
    /// <param name="source">Assumes <see cref="Source.EXTERNAL"/> by-default</param>
    public async Task<IStatusCodeActionResult> ValidateSession(Source source = Source.EXTERNAL)
    {
        string message = string.Empty;
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            message = $"{nameof(Session)} Validation Failed: No {nameof(HttpContext)} found.";
            await logging
                .LogError(message, m => {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        bool accountExists = httpContext.Items.TryGetValue(ACCOUNT_CONTEXT_KEY, out object? accountObj);
        if (accountExists)
        {
            var account = (Account)accountObj!;
            var getSession = await sessions.GetSessionByUser(account);
            var session = getSession.Value;

            if (session is not null)
            {
                return await ValidateSession(
                    account.Sessions.First(),
                    source
                );
            }
        }

        bool sessionCodeExists = httpContext.Items.TryGetValue(SESSION_CONTEXT_KEY, out object? sessionCodeObj);

        if (sessionCodeExists &&
            sessionCodeObj is string sessionCode &&
            !string.IsNullOrWhiteSpace(sessionCode)
        ) {
            return await ValidateSession(sessionCode, source);
        }

        message = $"Failed to infer a {nameof(Session)} from an {nameof(Account)} or Code in the {nameof(HttpContext)}";
        await logging
            .LogInformation(message, m => {
                m.Action = nameof(ValidateSession);
                m.Source = source;
            })
            .SaveAsync();
        
        return new UnauthorizedObjectResult(message);
    }
    /// <summary>
    /// Validates that a given <see cref="Session.Code"/> (string) is valid.
    /// </summary>
    public async Task<IStatusCodeActionResult> ValidateSession(string sessionCode, Source source = Source.INTERNAL)
    {
        var getSession = await sessions.GetSession(sessionCode);
        var session = getSession.Value;

        if (session is not null) {
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

        else if (getSession.Result is IStatusCodeActionResult statusCodeResult) {
            return statusCodeResult;
        }
        
        return new StatusCodeResult(StatusCodes.Status418ImATeapot);
    }
    /// <summary>
    /// Validates that a given <see cref="Session"/> is valid.
    /// </summary>
    public async Task<IStatusCodeActionResult> ValidateSession(Session session, Source source = Source.INTERNAL)
    {
        string message = string.Empty;
        var httpContext = contextAccessor.HttpContext;

        if (httpContext is null)
        {
            message = $"{nameof(Session)} Validation Failed: No {nameof(HttpContext)} found.";
            await logging
                .LogError(message, m => {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            message = $"{nameof(Session)} Validation Failed: Expired";
            await logging
                .LogInformation(message, m => {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (session.HasUserAgent)
        {
            if (session.UserAgent != httpContext.Request.Headers.UserAgent.ToString())
            {
                message = $"{nameof(Session)} Validation Failed: UserAgent missmatch";
                await logging
                    .LogSuspicious(message, m => {
                        m.Action = nameof(ValidateSession);
                        m.Source = source;
                    })
                    .SaveAsync();

                return new UnauthorizedObjectResult(
                    Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
                );
            }

            // Check if 24h expiry is valid..
            if (session.ExpiresAt > DateTime.UtcNow + TimeSpan.FromDays(1))
            {
                message = $"{nameof(Session)} Validation Failed: Invalid Expiry";
                await logging
                    .LogSuspicious(message, m => {
                        m.Action = nameof(ValidateSession);
                        m.Source = source;
                    })
                    .SaveAsync();

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
            await logging
                .LogSuspicious(message, m => {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        var getAccount = await sessions.GetUserBySession(session);
        var account = getAccount.Value;

        if (account is null)
        {
            message = $"{nameof(Session)} Validation Failed: Bad Account.";
            await logging
                .LogSuspicious(message, m => {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        message = $"{nameof(Session)} Validation Success";
        if (source == Source.INTERNAL)
        {
            logging.GetLogger().LogTrace(message);
            return new StatusCodeResult(StatusCodes.Status200OK);
        }

        await logging
            .LogTrace(message, m => {
                m.Action = nameof(ValidateSession);
                m.Source = source;
            })
            .SaveAsync();

        return new OkObjectResult(
            Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
        );
    }

    /// <summary>
    /// Attempt to "login" (..refresh the session) ..of a given <see cref="Account"/> and its hashed password.
    /// </summary>
    /// <param name="userName">Unique Username of an <see cref="Account"/></param>
    /// <param name="hash">SHA-256</param>
    public async Task<ActionResult<Session>> Login(string userName, string hash)
    {
        Account? account = await db.Accounts
            .Include(acc => acc.Sessions)
            .FirstOrDefaultAsync(acc => acc.Username == userName);

        if (account is null)
        {
            string message = $"Failed to find an {nameof(Account)} with Username '{userName}'.";
            await logging
                .Action(nameof(Login))
                .ExternalDebug(message)
                .SaveAsync();
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return await Login(account, hash);
    }
    /// <summary>
    /// Attempt to "login" (..refresh the session) ..of a given <see cref="Account"/> and its hashed password.
    /// </summary>
    /// <param name="hash">SHA-256</param>
    public async Task<ActionResult<Session>> Login(Account account, string hash)
    {
        if (account.Password != hash)
        {
            string message = $"Failed to login user '{account.Username}' (#{account.Id}). Password Missmatch.";
            await logging
                .Action(nameof(Login))
                .ExternalSuspicious(message)
                .SaveAsync();
            
            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        var createSession = await sessions.CreateSession(account, contextAccessor.HttpContext?.Request, Source.EXTERNAL);
        var session = createSession.Value;

        if (createSession.Result is NoContentResult)
        {
            logging
                .Action(nameof(Login))
                .ExternalTrace($"No new session created ({nameof(NoContentResult)})");

            var getSession = await sessions.GetSessionByUser(account);
            await db.SaveChangesAsync();

            return getSession;
        }
        else if (session is null)
        {
            string message = $"Failed to login user '{account.Username}' (#{account.Id}). Could not create a new {nameof(Session)} ({nameof(createSession.Result)}).";
            await logging
                .Action(nameof(Login))
                .ExternalDebug(message)
                .SaveAsync();
            
            return createSession.Result!;
        }

        return session;
    }
}