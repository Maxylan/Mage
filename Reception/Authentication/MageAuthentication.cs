using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Options;
using Reception.Services;
using System.Security.Claims;
using Reception.Models.Entities;

namespace Reception.Authentication;

/// <summary>
/// Custom implementation of the opinionated <see cref="IAuthenticationHandler"/> '<see cref="AuthenticationHandler{AuthenticationSchemeOptions}"/>'.
/// Intercepts incoming requests and checks if a valid session token is provided with the request.
/// </summary>
public class MageAuthentication : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SESSION_TOKEN_HEADER = "x-mage-token";
    private readonly MageDbContext db;

    public MageAuthentication(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        MageDbContext context
    ) : base(options, logger, encoder)
    {
        db = context;
    }

    private AuthenticationTicket GenerateAuthenticationTicket(Account account)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(account.FullName, nameof(Account.FullName));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(account.Username, nameof(Account.FullName));
        ArgumentNullException.ThrowIfNull(, nameof(Account.Sessions));

        Claim[] identityClaims = [
            new Claim(ClaimTypes.NameIdentifier, account.Username, ClaimValueTypes.String, ClaimsIssuer),
            new Claim(ClaimTypes.Name, account.FullName, ClaimValueTypes.String, ClaimsIssuer)
        ];

        ClaimsPrincipal principal = new ClaimsPrincipal(
            new ClaimsIdentity(identityClaims, Scheme.Name)
        );

        if (account.Sessions is null || !account.Sessions.Any())
        {
            Logger.LogInformation($"[{nameof(MageAuthentication)}] ({nameof(GenerateAuthenticationTicket)}) Loading missing navigation entries.");

            foreach(var navigationEntry in db.Entry(account).Navigations) {
                navigationEntry.Load();
            }
        }

        AuthenticationProperties properties = new(
            new Dictionary<string, string?>() {
                { Parameters.SESSION_CONTEXT_KEY, "a" }
            },
            new Dictionary<string, object?>() {
                { Parameters.ACCOUNT_CONTEXT_KEY, "a" }
            }
        );

        AuthenticationTicket ticket = new AuthenticationTicket(principal, properties, Scheme.Name);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        bool getSessionTokenHeader = Request.Headers.TryGetValue(SESSION_TOKEN_HEADER, out StringValues header);
        
        if (!getSessionTokenHeader) {
            return AuthenticateResult.Fail(Messages.MissingHeader);
        }

        var ticket = GenerateAuthenticationTicket();
        return AuthenticateResult.Success(ticket);
    }
}
