// using Microsoft.Extensions.Caching.Memory;
using Timer = System.Timers.Timer;
using System.Timers;
using Reception.Models;

namespace Reception.Caching;

// TODO! Overengineered (..or, under-engineered?)
public static class LoginTracker
{
    // TODO! Change to an `IMemoryCache` (works natively with ASP.NET)
    private static Dictionary<string, LoginAttempt> _cache = new();

    private static Timer? _timer = null;

    public static LoginAttempt? Get(Login login) =>
        Get(login.Username, login.Address ?? LoginAttempt.ADDR_FALLBACK);

    public static LoginAttempt? Get(string username, string remoteAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteAddress, nameof(remoteAddress));

        if (remoteAddress.Length > 255)
        {
            throw new ArgumentException($"Invalid {nameof(remoteAddress)}");
        }
        if (username.Length > 255)
        {
            throw new ArgumentException($"Invalid {nameof(username)}");
        }

        return Get(
            LoginAttempt.GetKey(username, remoteAddress)
        );
    }

    public static LoginAttempt? Get(string loginAttemptIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loginAttemptIdentifier);

        if (loginAttemptIdentifier.Length > 511)
        {
            throw new ArgumentException($"Invalid {nameof(loginAttemptIdentifier)}");
        }

        if (_cache.TryGetValue(loginAttemptIdentifier, out LoginAttempt attempt)) {
            return attempt;
        }

        return null;
    }


    public static uint Attempts(string username, string? remoteAddress) =>
        Get(username, remoteAddress ?? LoginAttempt.ADDR_FALLBACK)?.Attempt ?? 0;


    public static LoginAttempt Record(Login login) =>
        Record(login.Username, login.Address ?? LoginAttempt.ADDR_FALLBACK, login.UserAgent);

    public static LoginAttempt Record(string username, string? remoteAddress, string? userAgent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));

        if (username.Length > 255)
        {
            throw new ArgumentException($"Invalid {nameof(username)}");
        }

        if (string.IsNullOrWhiteSpace(remoteAddress))
        {
            remoteAddress = LoginAttempt.ADDR_FALLBACK;
        }
        else if (remoteAddress.Length > 255)
        {
            throw new ArgumentException($"Invalid {nameof(remoteAddress)}");
        }

        string loginIdentifier = LoginAttempt.GetKey(username, remoteAddress);
        LoginAttempt? existingLoginAttempt = Get(loginIdentifier);

        LoginAttempt newAttempt = existingLoginAttempt is null
            ? new LoginAttempt(1, username, remoteAddress, userAgent)
            : new LoginAttempt(
                    existingLoginAttempt.Value.Attempt + 1,
                    existingLoginAttempt.Value.Username,
                    existingLoginAttempt.Value.Address,
                    existingLoginAttempt.Value.UserAgent ?? userAgent
                );

        _cache[loginIdentifier] = newAttempt;

        if (_timer is null) {
            _timer = new Timer(TimeSpan.FromMinutes(15));
            _timer.Elapsed += ClearCache!;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        return newAttempt;
    }

    private static void ClearCache(Object source, ElapsedEventArgs e)
    {
        Console.WriteLine("Cache cleared at {0:HH:mm:ss.fff}", e.SignalTime);
        _cache.Clear();
    }
}
