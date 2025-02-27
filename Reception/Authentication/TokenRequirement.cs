using System.Net;
using Microsoft.AspNetCore.Authorization;
using Reception.Interfaces;

namespace Reception.Authentication;

public class TokenRequirement : IAuthorizationRequirement
{
    public string? Token { get; internal set; }
}

public class HandleTokenRequirement(
    IHttpContextAccessor contextAccessor,
    ILoggingService logging
) : AuthorizationHandler<TokenRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TokenRequirement requirement)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(HandleTokenRequirement)} Requirement Failed: No {nameof(HttpContext)} found.";
            await logging
                .Action(nameof(HandleTokenRequirement.HandleRequirementAsync))
                .ExternalError(message)
                .SaveAsync();

            context.Fail(
                new AuthorizationFailureReason(this, Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message)
            );

            return;
        }

        bool tryGetSessionTokenHeader = httpContext.Request.Headers.TryGetValue(Parameters.SESSION_TOKEN_HEADER, out var extractedToken);
        bool sessionTokenExists = !string.IsNullOrWhiteSpace(extractedToken);
        if (tryGetSessionTokenHeader || sessionTokenExists)
        {
            await logging
                .Action(nameof(HandleTokenRequirement.HandleRequirementAsync))
                .ExternalInformation(Messages.MissingHeader)
                .SaveAsync();

            context.Fail(
                new AuthorizationFailureReason(this, Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : Messages.MissingHeader)
            );

            return;
        }

        // Success!
        requirement.Token = extractedToken;
        context.Succeed(requirement);
    }
}