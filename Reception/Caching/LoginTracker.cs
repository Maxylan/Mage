using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Reception.Models;

namespace Reception.Caching;

public class LoginTracker
{
    private MemoryCache _cache;

    public LoginTracker(
        ILoggerFactory loggerFactory
    ) {
        this._cache = new(
            new MemoryCacheOptions() {
                ExpirationScanFrequency = TimeSpan.FromSeconds(6)
            },
            loggerFactory
        );
    }

    public LoginTracker(
        ILoggerFactory loggerFactory,
        IOptions<MemoryCacheOptions> optionsAccessor
    ) {
        this._cache = new(
            optionsAccessor,
            loggerFactory
        );
    }


    public LoginAttempt? GetAttempt(Login login) =>
        this.GetAttempt(login.Username, login.Address ?? LoginAttempt.ADDR_FALLBACK);

    public LoginAttempt? GetAttempt(string username, string remoteAddress) {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteAddress, nameof(remoteAddress));
        if (remoteAddress.Length > 255)
        {
            throw new ArgumentException($"Invalid {nameof(remoteAddress)}");
        }

        return this.GetAttempt(
            LoginAttempt.GetKey(username, remoteAddress)
        );
    }

    protected LoginAttempt? GetAttempt(string loginAttemptIdentifier) {
        ArgumentException.ThrowIfNullOrWhiteSpace(loginAttemptIdentifier);
        return this._cache.Get<LoginAttempt>(loginAttemptIdentifier);
    }


    public uint Attempts(string username, string? remoteAddress) =>
        this.GetAttempt(username, remoteAddress ?? LoginAttempt.ADDR_FALLBACK)?.Attempt ?? 0;


    public LoginAttempt RecordAttempt(Login login) =>
        this.RecordAttempt(login.Username, login.Address ?? LoginAttempt.ADDR_FALLBACK, login.UserAgent);

    public LoginAttempt RecordAttempt(string username, string? remoteAddress, string? userAgent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));

        if (string.IsNullOrWhiteSpace(remoteAddress))
        {
            remoteAddress = LoginAttempt.ADDR_FALLBACK;
        }
        else if (remoteAddress.Length > 255)
        {
            throw new ArgumentException($"Invalid {nameof(remoteAddress)}");
        }

        string? loginIdentifier = LoginAttempt.GetKey(username, remoteAddress);
        LoginAttempt? existingLoginAttempt = this.GetAttempt(loginIdentifier);
        LoginAttempt newAttempt = existingLoginAttempt is null
            ? new LoginAttempt(1, username, remoteAddress, userAgent)
            : new LoginAttempt(
                    existingLoginAttempt.Value.Attempt + 1,
                    existingLoginAttempt.Value.Username,
                    existingLoginAttempt.Value.Address,
                    existingLoginAttempt.Value.UserAgent ?? userAgent
                );

        var cacheEntry = this._cache.CreateEntry(loginIdentifier);
        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
        cacheEntry.Value = newAttempt;

        return newAttempt;
    }
}
