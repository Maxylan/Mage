using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models;
using Reception.Services;

namespace Reception.Interfaces;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly MageDbContext db;

    public LoggingService(ILogger<LoggingService> logger, MageDbContext context)
    {
        _logger = logger;
        db = context;
    }

    /// <summary>
    /// Get the <see cref="LogEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<LogEntry?>> GetEvent(uint id)
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
    public async Task<ActionResult<LogEntry>> LogTrace(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            LogLevel = DataTypes.Severity.TRACE
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> InternalTrace(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.INTERNAL,
            LogLevel = DataTypes.Severity.TRACE
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> ExternalTrace(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.EXTERNAL,
            LogLevel = DataTypes.Severity.TRACE
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> LogDebug(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            LogLevel = DataTypes.Severity.DEBUG
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> InternalDebug(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.INTERNAL,
            LogLevel = DataTypes.Severity.DEBUG
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> ExternalDebug(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.EXTERNAL,
            LogLevel = DataTypes.Severity.DEBUG
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> LogInformation(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            LogLevel = DataTypes.Severity.INFORMATION
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> InternalInformation(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.INTERNAL,
            LogLevel = DataTypes.Severity.INFORMATION
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> ExternalInformation(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.EXTERNAL,
            LogLevel = DataTypes.Severity.INFORMATION
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> LogSuspicious(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            LogLevel = DataTypes.Severity.SUSPICIOUS
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> InternalSuspicious(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.INTERNAL,
            LogLevel = DataTypes.Severity.SUSPICIOUS
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> ExternalSuspicious(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.EXTERNAL,
            LogLevel = DataTypes.Severity.SUSPICIOUS
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> LogWarning(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            LogLevel = DataTypes.Severity.WARNING
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> InternalWarning(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.INTERNAL,
            LogLevel = DataTypes.Severity.WARNING
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> ExternalWarning(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.EXTERNAL,
            LogLevel = DataTypes.Severity.WARNING
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> LogError(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            LogLevel = DataTypes.Severity.ERROR
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> InternalError(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.INTERNAL,
            LogLevel = DataTypes.Severity.ERROR
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> ExternalError(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.EXTERNAL,
            LogLevel = DataTypes.Severity.ERROR
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> LogCritical(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            LogLevel = DataTypes.Severity.CRITICAL
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> InternalCritical(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.INTERNAL,
            LogLevel = DataTypes.Severity.CRITICAL
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> ExternalCritical(Action<LogEntry> predicate)
    {
        LogEntry entry = new() {
            Source = DataTypes.Source.EXTERNAL,
            LogLevel = DataTypes.Severity.CRITICAL
        };

        predicate(entry);
        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> LogEvent(Action<LogEntry> predicate)
    {
        LogEntry entry = new();
        predicate(entry);

        return await LogEvent(entry);
    }
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public async Task<ActionResult<LogEntry>> LogEvent(LogEntry entry)
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

        await db.SaveChangesAsync();
        return entry;
    }
#endregion

    /// <summary>
    /// Deletes all provided <see cref="LogEntry"/>-entries.
    /// </summary>
    public async Task<uint> DeleteEvents(params LogEntry[] entries)
    {
        throw new NotImplementedException();
    }
}