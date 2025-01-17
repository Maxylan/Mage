using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Reception.Middleware;

public class AuthenticationInterceptor() : AuthenticationHandler<AuthenticationSchemeOptions>
{

    public ApiKeyRequirement()
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        throw new NotImplementedException();
    }
}
