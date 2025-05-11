using Reception.Utilities;

namespace Reception.Models;

public class Login
{
    /// <summary>
    /// Your <c><see cref="Reception.Database.Models.Account.Username"/></c>.
    /// </summary>
    public string Username { get; init; } = null!;

    /// <summary>
    /// Your <c><see cref="Reception.Database.Models.Account.Password"/></c>.
    /// </summary>
    public string Hash { get; init; } = null!;

    /// <summary>
    /// Your <c>IP Address</c>.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Your <c>HTTP Client User Agent</c>.
    /// </summary>
    public string? UserAgent { get; init; }
}

public struct LoginAttempt
{
    public const string ADDR_FALLBACK = "unknown";
    public static string GetKey(string username, string? address) {
        if (string.IsNullOrWhiteSpace(address)) {
            return $"{username}_{ADDR_FALLBACK}";

        }

        return $"{username}_{address}";
    }

    public readonly uint Attempt { get; init; } = 0;
    public readonly string Username { get; init; }
    public readonly string? Address { get; init; }
    public readonly string? UserAgent { get; init; }

    public LoginAttempt(
        uint attempt,
        string username,
        string? address,
        string? userAgent
    ) {
        this.Attempt = attempt;
        this.Username = username.ToLower().Replace(" ", "-");

        if (string.IsNullOrWhiteSpace(this.Username)) {
            throw new ArgumentException($"Argument {nameof(username)} cannot be null/empty");
        }
        if (this.Username.Length > 63) {
            throw new ArgumentException($"Argument {nameof(username)} cannot be execed 63 characters");
        }

        this.Address = address?.ToLower()
            .Replace(" ", "-")
            .Subsmart(0, 255);

        this.UserAgent = userAgent?.ToLower()
            .Replace(" ", "-")
            .Subsmart(0, 1023);
    }

    public LoginAttempt(uint attempt, Login login) {
        this.Attempt = attempt;
        this.Username = login.Username.ToLower().Replace(" ", "-");

        if (string.IsNullOrWhiteSpace(this.Username)) {
            throw new ArgumentException($"Argument {nameof(login.Username)} cannot be null/empty");
        }
        if (this.Username.Length > 63) {
            throw new ArgumentException($"Argument {nameof(login.Username)} cannot be execed 63 characters");
        }

        this.Address = login.Address?.ToLower()
            .Replace(" ", "-")
            .Subsmart(0, 255);

        this.UserAgent = login.UserAgent?.ToLower()
            .Replace(" ", "-")
            .Subsmart(0, 1023);
    }

    public string Key => LoginAttempt.GetKey(this.Username, this.Address);
}
