
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Reception.Interfaces;
using Reception.Models;

namespace Reception.Caching;

public class LoginTracker : MemoryCache
{
    private readonly ILogger<LoginTracker> logger;

    public LoginTracker(
        ILoggerFactory loggerFactory
    ) : base(
        new MemoryCacheOptions() {
            ExpirationScanFrequency = TimeSpan.FromSeconds(6)
        },
        loggerFactory
    ) {
        this.logger = loggerFactory.CreateLogger<LoginTracker>();
    }

    public LoginTracker(
        ILoggerFactory loggerFactory,
        IOptions<MemoryCacheOptions> optionsAccessor
    ) : base(
        optionsAccessor,
        loggerFactory
    ) {
        this.logger = loggerFactory.CreateLogger<LoginTracker>();
    }


    public LoginAttempt? GetByAddress(string username, string remoteAddress) => 
        this.Get<LoginAttempt>(LoginAttempt.KeyFormat(username, remoteAddress));
    public LoginAttempt? GetByAddress(LoginAttempt login)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(login.Username, nameof(LoginAttempt.Username));
        // ArgumentException.ThrowIfNullOrWhiteSpace(login.Address, nameof(LoginAttempt.Address));
        if (string.IsNullOrWhiteSpace(login.Address)) {
            return null;
        }

        return this.Get<LoginAttempt>(LoginAttempt.KeyFormat(login.Username, login.Address));
    }

    public LoginAttempt? GetByUserAgent(string username, string userAgent) => 
        this.Get<LoginAttempt>(LoginAttempt.KeyFormat(username, userAgent));
    public LoginAttempt? GetByUserAgent(LoginAttempt login)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(login.Username, nameof(LoginAttempt.Username));
        // ArgumentException.ThrowIfNullOrWhiteSpace(login.UserAgent, nameof(LoginAttempt.UserAgent));
        if (string.IsNullOrWhiteSpace(login.UserAgent)) {
            return null;
        }

        return this.Get<LoginAttempt>(LoginAttempt.KeyFormat(login.Username, login.UserAgent));
    }


    public void Set(LoginAttempt login)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(login.Username, nameof(LoginAttempt.Username));

        if (string.IsNullOrWhiteSpace(login.Address) && string.IsNullOrWhiteSpace(login.UserAgent))
        {
            string message = $"'{nameof(LoginTracker.Set)}' Requires either {nameof(LoginAttempt.Address)} or {nameof(LoginAttempt.UserAgent)} to have a non-empty value.";
            var exception = new InvalidOperationException(message);
            logger.LogError(
                exception, $"[{nameof(LoginTracker)}] {message}"
            );
            
            throw exception;
        }

        if (!string.IsNullOrWhiteSpace(login.Address))
        {
            var existingLoginAttempt = this.GetByAddress(login);
            existingLoginAttempt ??= login;

            var cacheEntry = base.CreateEntry(login.AddressKey);
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            cacheEntry.Value = existingLoginAttempt! with {
                UserAgent = existingLoginAttempt.UserAgent ?? login.UserAgent,
                Attempts = existingLoginAttempt.Attempts + 1
            };
        }

        if (!string.IsNullOrWhiteSpace(login.UserAgent))
        {
            var existingLoginAttempt = this.GetByUserAgent(login);
            existingLoginAttempt ??= login;

            var cacheEntry = base.CreateEntry(login.UserAgentKey);
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            cacheEntry.Value = existingLoginAttempt! with {
                UserAgent = existingLoginAttempt.Address ?? login.Address,
                Attempts = existingLoginAttempt.Attempts + 1
            };
        }
    }


    public uint Attempts(LoginAttempt login)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(login.Username, nameof(LoginAttempt.Username));

        if (string.IsNullOrWhiteSpace(login.Address) && string.IsNullOrWhiteSpace(login.UserAgent))
        {
            string message = $"'{nameof(LoginTracker.Attempts)}' Requires either {nameof(LoginAttempt.Address)} or {nameof(LoginAttempt.UserAgent)} to have a non-empty value.";
            var exception = new InvalidOperationException(message);
            logger.LogError(
                exception, $"[{nameof(LoginTracker)}] {message}"
            );
            
            throw exception;
        }

        return this.Get<LoginAttempt?>(login.Key)?.Attempts ?? 0;
    }
}