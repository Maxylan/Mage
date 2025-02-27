namespace Reception.Authentication;

/// <summary>
/// Static collection of hardcoded parameter key names (<see cref="Microsoft.AspNetCore.Authentication.AuthenticationProperties"/>)
/// </summary>
public static class Parameters
{
    public const string AUTHENTICATED_POLICY = "Authenticated";
    public const string SESSION_TOKEN_HEADER = "x-mage-token";
    public const string SCHEME = "mage-authentication";
    public const string TOKEN_CONTEXT_KEY = "token";
    public const string SESSION_CONTEXT_KEY = "session";
    public const string ACCOUNT_CONTEXT_KEY = "account";
}