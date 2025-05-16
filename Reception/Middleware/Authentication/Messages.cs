using Microsoft.AspNetCore.Authorization;
using Reception.Database.Models;

namespace Reception.Middleware.Authentication;

/// <summary>
/// Static collection of hardcoded response values
/// </summary>
public static class Messages
{
    public static string ByCode(int code) => code switch
    {
        UnknownErrorCode => UnknownError,
        UnauthorizedCode => Unathorized,
        MissingHeaderCode => MissingHeader,
        MissingSessionCode => MissingSession,
        MissingAuthorizationResultCode => MissingAuthorizationResult,
        ValidationFailedCode => ValidationFailed,
        _ => UnknownError,
    };
    public static string ByCode(IAuthenticationException authException) =>
        ByCode(authException.Code);

    private static string Prefix(int code) =>
        $"Code {code}: ";

    public const int UnknownErrorCode = 1;
    public static string UnknownError => Prefix(UnknownErrorCode) + (
        Program.IsProduction ? "Unknown Server Error." : $"Unknown Server Error."
    );

    public const int UnauthorizedCode = 2;
    public static string Unathorized => Prefix(UnauthorizedCode) + (
        Program.IsProduction ? "User is unauthorized." : $"User is unauthorized. Maybe missing a valid {nameof(Microsoft.AspNetCore.Authorization.AuthorizationResult)}?."
    );

    public const int MissingHeaderCode = 3;
    public static string MissingHeader => Prefix(MissingHeaderCode) + (
        Program.IsProduction ? "No Authentication Provided." : $"Missing {nameof(Constants.SESSION_TOKEN_HEADER)} Authentication Header."
    );

    public const int MissingSessionCode = 4;
    public static string MissingSession => Prefix(MissingSessionCode) + (
        Program.IsProduction ? $"{nameof(Account)} is missing a session." : $"User {nameof(Account)} is missing a session."
    );

    public const int MissingAuthorizationResultCode = 5;
    public static string MissingAuthorizationResult => Prefix(MissingAuthorizationResultCode) + (
        Program.IsProduction ? "Internal Server Error." : $"An {nameof(AuthorizationResult)} instance was expected, but found missing in our current {nameof(HttpContext)}."
    );

    public const int ValidationFailedCode = 6;
    public static string ValidationFailed => Prefix(ValidationFailedCode) + (
        Program.IsProduction ? "Token Validation Failed." : $"{nameof(Session)} Token Validation Failed."
    );
}
