using System.Net;
using Microsoft.AspNetCore.Authorization;
using Reception.Interfaces;

namespace Reception.Middleware.Authentication;

public class TokenRequirement : IAuthorizationRequirement
{
    public string? Token { get; internal set; }
}

public class HandleTokenRequirement(
    IHttpContextAccessor contextAccessor,
    ILoggingService<MemoAuth> logging
) : AuthorizationHandler<TokenRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TokenRequirement requirement)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(HandleTokenRequirement)} Requirement Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(HandleTokenRequirement.HandleRequirementAsync))
                .ExternalError(message)
                .LogAndEnqueue();

            context.Fail(
                new AuthorizationFailureReason(this, Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message)
            );

            return Task.CompletedTask;
        }

        bool tryGetSessionTokenHeader = httpContext.Request.Headers.TryGetValue(Constants.SESSION_TOKEN_HEADER, out var extractedToken);
        bool sessionTokenExists = !string.IsNullOrWhiteSpace(extractedToken);
        if (tryGetSessionTokenHeader || sessionTokenExists)
        {
            logging
                .Action(nameof(HandleTokenRequirement.HandleRequirementAsync))
                .ExternalInformation(Messages.MissingHeader)
                .LogAndEnqueue();

            context.Fail(
                new AuthorizationFailureReason(this, Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : Messages.MissingHeader)
            );

            return Task.CompletedTask;
        }

        // Success!
        requirement.Token = extractedToken;
        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
