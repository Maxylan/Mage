using Reception.Interfaces;
using Reception.Services;

namespace Reception.Models;

public class Login
{
    /// <summary>
    /// Your <c><see cref="Reception.Models.Entities.Account.Username"/></c>.
    /// </summary>
    public string Username { get; init; } = null!;

    /// <summary>
    /// Your <c><see cref="Reception.Models.Entities.Account.Password"/></c>.
    /// </summary>
    public string Hash { get; init; } = null!;
}

public record class LoginAttempt
{
    public LoginAttempt(string username)
    {
        Username = username;
    }

    public string? UserAgent { get; init; }
    public string? Address { get; init; }
    public string Username { get; init; }
    public uint Attempts { get; init; } = 0;

    public string Key
    {
        get
        {
            string? deviceIdentifier = Address ?? UserAgent;
            ArgumentException.ThrowIfNullOrWhiteSpace(deviceIdentifier, nameof(deviceIdentifier));

            return LoginAttempt.KeyFormat(this.Username, deviceIdentifier);
        }
    }
    public string AddressKey
    {
        get
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(this.Address, nameof(this.Address));
            return LoginAttempt.KeyFormat(this.Username, this.Address);
        }
    }
    public string UserAgentKey
    {
        get
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(this.UserAgent, nameof(this.UserAgent));
            return LoginAttempt.KeyFormat(this.Username, this.UserAgent);
        }
    }

    public static string KeyFormat(string username, string deviceIdentifier) =>
        $"{username}_{deviceIdentifier}";
}