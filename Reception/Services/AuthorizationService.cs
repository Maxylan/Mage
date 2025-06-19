using System.Net;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Reception.Middleware.Authentication;
using Reception.Database;
using Reception.Database.Models;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Utilities;
using Reception.Caching;
using Reception.Models;

namespace Reception.Services;

public class AuthorizationService(
    IHttpContextAccessor contextAccessor,
    ILoggingService<AuthorizationService> logging,
    ISessionService sessionService,
    IClientService clientService,
    IBannedClientsService banService,
    IAccountService accountService
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

        bool getAuthenticationProperties = MemoAuth.TryGetProperties(httpContext, out AuthenticationProperties? authenticationProperties);
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

        var user = authenticationProperties!.GetParameter<Account>(
            Reception.Middleware.Authentication.Constants.ACCOUNT_CONTEXT_KEY
        );
        if (user is not null)
        {
            var getSession = await sessionService.GetSessionByUser(user);
            if (getSession.Value is not null)
            {
                return await ValidateSession(
                    getSession.Value,
                    source
                );
            }
        }

        var session = authenticationProperties!.GetParameter<Session>(
            Reception.Middleware.Authentication.Constants.SESSION_CONTEXT_KEY
        );
        if (session is not null)
        {
            return await ValidateSession(
                session,
                source
            );
        }

        bool tokenExists = authenticationProperties!.Items.TryGetValue(
            Reception.Middleware.Authentication.Constants.TOKEN_CONTEXT_KEY,
            out string? token
        );
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
        var getSession = await sessionService.GetSession(sessionCode);
        var session = getSession.Value;

        if (session is not null)
        {
            return await ValidateSession(session, source);
        }

        if (getSession.Result is NotFoundObjectResult)
        {
            string message = $"Failed to get a {nameof(Session)} from the {nameof(sessionCode)} '{nameof(sessionCode)}'";
            logging
                .LogInformation(message, m =>
                {
                    m.Action = nameof(ValidateSession);
                    m.Source = source;
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsDevelopment ? message : HttpStatusCode.Unauthorized.ToString()
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
                .Action(nameof(ValidateSession))
                .LogError(message, m =>
                {
                    m.Source = source;
                    m.SetUser(session.Account);
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            message = $"{nameof(Session)} Validation Failed: Expired (Expired @'{session.ExpiresAt.ToString()}')";
            logging
                .Action(nameof(ValidateSession))
                .LogInformation(message, m =>
                {
                    m.Source = source;
                    m.SetUser(session.Account);
                })
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (session.Client is not null &&
            !string.IsNullOrWhiteSpace(session.Client.Address) &&
            !string.IsNullOrWhiteSpace(session.Client.UserAgent)
        ) {
            string? userAgent = httpContext.Request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                userAgent = userAgent
                    .Normalize()
                    .Subsmart(0, 1023)
                    .Replace(@"\", string.Empty)
                    .Replace("&&", string.Empty)
                    .Trim();
            }

            if (session.Client.UserAgent != userAgent)
            {
                message = $"{nameof(Session)} Validation Failed: UserAgent missmatch ({session.Client.UserAgent} != {userAgent})";
                logging
                    .Action(nameof(ValidateSession))
                    .LogSuspicious(message, m =>
                    {
                        m.Source = source;
                        m.SetUser(session.Account);
                    })
                    .LogAndEnqueue();

                return new UnauthorizedObjectResult(
                    Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
                );
            }

            var checkIsBanned = await clientService.IsBanned(session.Client);
            BanEntry? banEntry = checkIsBanned.Value;

            if (banEntry is not null)
            {
                message = $"{nameof(Session)} Validation Failed: Client ({session.Client.Id}) is banned.";
                logging
                    .Action(nameof(ValidateSession))
                    .LogInformation(message, m =>
                    {
                        m.Source = source;
                        m.SetUser(session.Account);
                    })
                    .LogAndEnqueue();

                return new UnauthorizedObjectResult(
                    Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
                );
            }

            string? userAddress = MemoAuth.GetRemoteAddress(httpContext);
            if (!string.IsNullOrWhiteSpace(userAddress))
            {
                userAddress = userAddress
                    .Normalize()
                    .Subsmart(0, 255)
                    .Replace(@"\", string.Empty)
                    .Replace("&&", string.Empty)
                    .Trim();
            }

            if (session.Client.Address != userAddress)
            {
                message = $"{nameof(Session)} Validation Failed: Address missmatch ({session.Client.Address} != {userAddress})";
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
            if (session.ExpiresAt > (DateTime.UtcNow + TimeSpan.FromDays(1)))
            {
                message = $"{nameof(Session)} Validation Failed: Invalid Expiry (Expiry @'{session.ExpiresAt.ToString()}')";
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
        else if (session.ExpiresAt > (DateTime.UtcNow + TimeSpan.FromHours(1)))
        {
            message = $"{nameof(Session)} Validation Failed: Invalid Expiry (Expiry @'{session.ExpiresAt.ToString()}')";
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

        var getAccount = await sessionService.GetUserBySession(session);
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
        else if (session.Account is null)
        {
            session.Account = account;
        }

        logging
            .Action(nameof(ValidateSession))
            .ExternalTrace($"Validation of {nameof(Session)} '{session}' successful")
            .LogAndEnqueue();

        return session;
    }

    /// <summary>
    /// Attempt to "login" (..refresh the session) ..of a given <see cref="Account"/> and its hashed password.
    /// </summary>
    /// <param name="userName">Unique Username of an <see cref="Account"/></param>
    /// <param name="password">SHA-256 Digest of a password</param>
    public async Task<ActionResult<Session>> Login(string userName, string password)
    {
        var getAccount = await accountService.GetAccountByUsername(userName);
        Account? account = getAccount.Value;

        if (account is null)
        {
            // To rate-limit password attempts, even in this fail-fast check..
            Thread.Sleep(512);
            logging
                .Action(nameof(Login))
                .ExternalDebug($"Login Failed: No {nameof(Account)} matching username '{userName}' found.")
                .LogAndEnqueue();

            return getAccount.Result!;
        }

        return await Login(account, password);
    }
    /// <summary>
    /// Attempt to "login" (..refresh the session) ..of a given <see cref="Account"/> and its hashed password.
    /// </summary>
    /// <param name="password">SHA-256 Digest of a password</param>
    public async Task<ActionResult<Session>> Login(Account account, string password)
    {
        // To sortof rate-limit password attempts..
        Thread.Sleep(512);

        if (contextAccessor.HttpContext is null)
        {
            string message = $"Login Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(Login))
                .ExternalError(message)
                .LogAndEnqueue();

            return new ObjectResult(
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : message
            ) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        if (!account.Username.IsNormalized())
        {
            account.Username = account.Username
                .Normalize()
                .Trim();
        }
        if (account.Username.Length > 127)
        {
            string message = $"{nameof(Account.Username)} exceeds maximum allowed length of 127.";
            logging
                .Action(nameof(Login))
                .ExternalSuspicious(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (!account.Password.IsNormalized())
        {
            account.Password = account.Password
                .Normalize()
                .Trim();
        }
        if (account.Password.Length > 127)
        {
            string message = $"{nameof(Account.Password)} exceeds maximum allowed length of 127.";
            logging
                .Action(nameof(Login))
                .ExternalSuspicious(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (!string.IsNullOrWhiteSpace(account.Email))
        {
            if (!account.Email.IsNormalized())
            {
                account.Email = account.Email
                    .Normalize()
                    .Trim();
            }
            if (account.Email.Length > 255)
            {
                string message = $"{nameof(Account.Email)} exceeds maximum allowed length of 255.";
                logging
                    .Action(nameof(Login))
                    .ExternalSuspicious(message)
                    .LogAndEnqueue();

                return new BadRequestObjectResult(message);
            }
        }
        else {
            account.Email = null;
        }

        if (!string.IsNullOrWhiteSpace(account.FullName))
        {
            if (!account.FullName.IsNormalized())
            {
                account.FullName = account.FullName
                    .Normalize()
                    .Trim();
            }
            if (account.FullName.Length > 255)
            {
                string message = $"{nameof(Account.FullName)} exceeds maximum allowed length of 255.";
                logging
                    .Action(nameof(Login))
                    .ExternalSuspicious(message)
                    .LogAndEnqueue();

                return new BadRequestObjectResult(message);
            }
        }
        else {
            account.FullName = null;
        }

        string? userAddress = MemoAuth.GetRemoteAddress(contextAccessor.HttpContext);
        if (!string.IsNullOrWhiteSpace(userAddress))
        {
            userAddress = userAddress
                .Normalize()
                .Subsmart(0, 255)
                .Replace(@"\", string.Empty)
                .Replace("&&", string.Empty)
                .Trim();
        }

        string? userAgent = contextAccessor.HttpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = userAgent
                .Normalize()
                .Subsmart(0, 1023)
                .Replace(@"\", string.Empty)
                .Replace("&&", string.Empty)
                .Trim();
        }

        if (LoginTracker.Attempts(account.Username, userAddress) >= 3)
        {
            string message = $"Failed to login user '{account.Username}' (#{account.Id}). Timeout for addr '{userAddress}' due to repeatedly failed attempts.";
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

        byte[] digest = SHA256.HashData(
            Encoding.Default.GetBytes(password)
        );

        string hashedPassword = string.Join(
            string.Empty,
            digest.Select(@byte => @byte.ToString("x2"))
        );

        if (account.Password != hashedPassword)
        {
            LoginTracker.Record(account.Username, userAddress, userAgent);

            string message = $"Failed to login user '{account.Username}' (#{account.Id}). Password Missmatch.";
            logging
                .Action(nameof(Login))
                .ExternalSuspicious(message)
                .LogAndEnqueue();

            await sessionService.CleanupSessions();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        var createSession = await sessionService.CreateSession(account, contextAccessor.HttpContext.Request, Source.EXTERNAL);
        var session = createSession.Value;

        if (createSession.Result is NoContentResult)
        {
            logging
                .Action(nameof(Login))
                .ExternalTrace($"No new session created ({nameof(NoContentResult)})")
                .LogAndEnqueue();

            var getSession = await sessionService.GetSessionByUser(account);
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

    /// <summary>
    /// Attempt to ban a client. By default the ban is indefinite, but you may optionally provide a
    /// <see cref="DateTime"/> as <paramref name="expiry"/>
    /// </summary>
    public async Task<ActionResult<BanEntry>> BanClientByFingerprint(string address, string? userAgent, DateTime? expiry = null)
    {
        if (contextAccessor.HttpContext is null)
        {
            string message = $"BanClient Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(BanClientByFingerprint))
                .ExternalError(message)
                .LogAndEnqueue();

            return new ObjectResult(
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : message
            ) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        Account? user;
        try
        {
            user = MemoAuth.GetAccount(contextAccessor);

            if (user is null) {
                return new ObjectResult("Prevented attempted unauthorized access.") {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
        catch (Exception ex)
        {
            string message = $"Potential Unauthorized attempt at ${nameof(BanClientByFingerprint)}. Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(BanClientByFingerprint))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if ((user.Privilege & Privilege.ADMIN) != Privilege.ADMIN)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({Privilege.ADMIN}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(BanClientByFingerprint))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        var getClient = await clientService.GetClientByFingerprint(address, userAgent);
        Client? client = getClient.Value;

        if (client is null)
        {
            string message = $"Failed to find a {nameof(Client)} matching the given fingerprint.";
            logging
                .Action(nameof(BanClientByFingerprint))
                .ExternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return await this.BanClient(client, expiry);
    }

    /// <summary>
    /// Attempt to ban a client. By default the ban is indefinite, but you may optionally provide a
    /// <see cref="DateTime"/> as <paramref name="expiry"/>
    /// </summary>
    public async Task<ActionResult<BanEntry>> BanClient(Client client, DateTime? expiry = null, string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(client);

        Account? user;
        try
        {
            user = MemoAuth.GetAccount(contextAccessor);

            if (user is null) {
                return new ObjectResult("Prevented attempted unauthorized access.") {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
        catch (Exception ex)
        {
            string message = $"Potential Unauthorized attempt at ${nameof(BanClientByFingerprint)}. Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(BanClient))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if ((user.Privilege & Privilege.ADMIN) != Privilege.ADMIN)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({Privilege.ADMIN}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(BanClient))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if (client.Id <= 0)
        {
            string message = $"Parameter {nameof(client.Id)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(AuthorizationService.BanClient))
                .LogDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (expiry is null)
        {
            expiry = DateTime.UtcNow.AddMonths(3);
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            reason = reason
                .Normalize()
                .Trim();

            reason = reason.Subsmarter(0, 32768);
        }

        MutateBanEntry mut = new() {
            ClientId = client.Id,
            ExpiresAt = expiry,
            Reason = reason
        };

        var banEntry = await banService.CreateBanEntry(mut);

        if (banEntry is null)
        {
            string message = $"Failed to find a {nameof(Client)} matching the given fingerprint.";
            logging
                .Action(nameof(BanClient))
                .LogDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return banEntry;
    }
}
