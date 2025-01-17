namespace Reception.Authentication;

/// <summary>
/// Static collection of hardcoded response values
/// </summary>
public static class Messages
{
    public static string MissingHeader => 
        Program.IsProduction ? "No Authentication Provided" : $"Missing {nameof(MageAuthentication.SESSION_TOKEN_HEADER)} Authentication Header.";
}