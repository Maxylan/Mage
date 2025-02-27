using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models;
using Reception.Interfaces;
using Reception.Properties;

namespace Reception.Services;

public class LoggingService : ILoggingService
{
    private readonly IHttpContextAccessor contextAccessor;
    private readonly ILogger<LoggingService> logger;
    private readonly MageDbContext db;

    public LoggingService(ILogger<LoggingService> logger, IHttpContextAccessor httpContextAccessor, MageDbContext dbContext)
    {
        contextAccessor = httpContextAccessor;
        this.logger = logger;
        db = dbContext;
    }

    /// <summary>
    /// Get the current user's <see cref="Account"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    private Account? GetAccount()
    {
        if (contextAccessor.HttpContext is not null &&
            contextAccessor.HttpContext.Items.TryGetValue(Constants.HttpContext.CURRENT_USER, out object? currentUser)
        ) {
            return (Account) currentUser!;
        }

        return null;
    }

    /// <summary>
    /// Get the <see cref="LogEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<LogEntry?>> GetEvent(int id)
    {
        LogEntry? result = await db.Logs.FindAsync(id);
        if (result is null) {
            return new NotFoundObjectResult($"Failed to find {nameof(LogEntry)} with ID {id}");
        }

        return result;
    }

    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;LogEntry&gt;"/>) set of 
    /// <see cref="LogEntry"/>-entries, you may use it to freely fetch some logs.
    /// </summary>
    public DbSet<LogEntry> GetEvents() => db.Logs;

#region Create Logs (w/ many shortcuts)
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> LogTrace(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.LogLevel = DataTypes.Severity.TRACE;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> InternalTrace(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.INTERNAL;
            entry.LogLevel = DataTypes.Severity.TRACE;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> ExternalTrace(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.EXTERNAL;
            entry.LogLevel = DataTypes.Severity.TRACE;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> LogDebug(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.LogLevel = DataTypes.Severity.DEBUG;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> InternalDebug(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.INTERNAL;
            entry.LogLevel = DataTypes.Severity.DEBUG;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> ExternalDebug(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.EXTERNAL;
            entry.LogLevel = DataTypes.Severity.DEBUG;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> LogInformation(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.LogLevel = DataTypes.Severity.INFORMATION;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> InternalInformation(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.INTERNAL;
            entry.LogLevel = DataTypes.Severity.INFORMATION;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> ExternalInformation(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.EXTERNAL;
            entry.LogLevel = DataTypes.Severity.INFORMATION;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> LogSuspicious(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.LogLevel = DataTypes.Severity.SUSPICIOUS;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> InternalSuspicious(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.INTERNAL;
            entry.LogLevel = DataTypes.Severity.SUSPICIOUS;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> ExternalSuspicious(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.EXTERNAL;
            entry.LogLevel = DataTypes.Severity.SUSPICIOUS;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> LogWarning(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.LogLevel = DataTypes.Severity.WARNING;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> InternalWarning(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.INTERNAL;
            entry.LogLevel = DataTypes.Severity.WARNING;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> ExternalWarning(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.EXTERNAL;
            entry.LogLevel = DataTypes.Severity.WARNING;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> LogError(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.LogLevel = DataTypes.Severity.ERROR;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> InternalError(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.INTERNAL;
            entry.LogLevel = DataTypes.Severity.ERROR;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> ExternalError(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.EXTERNAL;
            entry.LogLevel = DataTypes.Severity.ERROR;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> LogCritical(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.LogLevel = DataTypes.Severity.CRITICAL;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> InternalCritical(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.INTERNAL;
            entry.LogLevel = DataTypes.Severity.CRITICAL;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> ExternalCritical(string message, Action<LogEntry>? predicate = null) => 
        await LogNewEvent(message, entry => {
            entry.Source = DataTypes.Source.EXTERNAL;
            entry.LogLevel = DataTypes.Severity.CRITICAL;
        });
    
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<int> LogNewEvent(string message, Action<LogEntry>? predicate = null)
    {
        LogEntry entry = new() {
            Log = message,
            CreatedAt = DateTime.UtcNow
        };

        Account? user = GetAccount();
        if (user is not null) {
            entry.UserId = user.Id;
            entry.UserUsername = user.Username;
            entry.UserFullName = user.Username;
            entry.UserEmail = user.Email;
        }

        if (predicate is not null) {
            predicate(entry);
        }

        if (string.IsNullOrWhiteSpace(entry.Action)) {
            entry.Action = "Unknown";
        }
        
        return await LogEvents(entry);
    }

    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event, attempts to add it to the database, but does 
    /// <strong>not</strong> call <see cref="DbContext.SaveChangesAsync"/>.
    /// </summary>
    /// <remarks>
    /// Note: Shares a similar name with <seealso cref="ILoggingService.LogEvents"/>, but the two methods couldn't be more different.
    /// </remarks>
    public async Task LogEvent(LogEntry entry)
    {
        if (db.Entry(entry).State != EntityState.Added)
        {
            switch(await db.Logs.ContainsAsync(entry)) {
                case true: // Exists
                    db.Update(entry);
                    break;
                case false: // New
                    db.Add(entry);
                    break;
            }
        }

        switch(entry.LogLevel) {
            case DataTypes.Severity.TRACE:
                logger.LogTrace(entry.Format.Short(false));
                break;
            case DataTypes.Severity.DEBUG:
                logger.LogDebug(entry.Format.Short());
                break;
            case DataTypes.Severity.SUSPICIOUS:
                logger.LogWarning(entry.Format.Standard());
                break;
            case DataTypes.Severity.WARNING:
                logger.LogWarning(entry.Format.Standard());
                break;
            case DataTypes.Severity.ERROR:
                logger.LogError(entry.Format.Standard());
                break;
            case DataTypes.Severity.CRITICAL:
                logger.LogCritical(entry.Format.Standard());
                break;
            default: // DataTypes.Severity.INFORMATION:
                logger.LogInformation(entry.Format.Short(true, true));
                break;
        }
    }
    /// <summary>
    /// Log any number of custom <see cref="LogEntry"/>-events to the database.
    /// </summary>
    /// <remarks>
    /// Plural name (<see cref="ILoggingService.LogEvents"/>), but you can do one/single entry, or many (plural)
    /// </remarks>
    public async Task<int> LogEvents(params LogEntry[] entries)
    {
        foreach(var entry in entries) {
            await LogEvent(entry);
        }

        return await db.SaveChangesAsync();
    }
#endregion

    /// <summary>
    /// Deletes all provided <see cref="LogEntry"/>-entries.
    /// </summary>
    public async Task<int> DeleteEvents(params LogEntry[] entries)
    {
        foreach(var entry in entries)
        {
            bool exists = await db.Logs.ContainsAsync(entry);

            if (exists)
            {
                db.Remove(entry);
            }
            else {
                // Stop tracking the entity, this should result in nothing being added on next save, effectively deleting the entity, without a database call.
                db.Entry(entry).State = EntityState.Detached;
            }
        }

        return await db.SaveChangesAsync();
    }
}