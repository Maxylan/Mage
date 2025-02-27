
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models;

namespace Reception.Interfaces;

public interface ILoggingService
{
    /// <summary>
    /// Get the <see cref="LogEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<LogEntry?>> GetEvent(int id);
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;LogEntry&gt;"/>) set of 
    /// <see cref="LogEntry"/>-entries, you may use it to freely fetch some logs.
    /// </summary>
    public abstract DbSet<LogEntry> GetEvents();

    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> LogTrace(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> InternalTrace(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> ExternalTrace(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> LogDebug(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> InternalDebug(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> ExternalDebug(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> LogInformation(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> InternalInformation(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> ExternalInformation(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> LogSuspicious(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> InternalSuspicious(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> ExternalSuspicious(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> LogWarning(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> InternalWarning(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> ExternalWarning(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> LogError(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> InternalError(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> ExternalError(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> LogCritical(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> InternalCritical(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> ExternalCritical(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract Task<int> LogNewEvent(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event, attempts to add it to the database, but does 
    /// <strong>not</strong> call <see cref="DbContext.SaveChangesAsync"/>.
    /// </summary>
    /// <remarks>
    /// Note: Shares a similar name with <seealso cref="ILoggingService.LogEvents"/>, but the two methods couldn't be more different.
    /// </remarks>
    public abstract Task LogEvent(LogEntry entry);
    /// <summary>
    /// Log any number of custom <see cref="LogEntry"/>-events to the database.
    /// </summary>
    /// <remarks>
    /// Plural name (<see cref="ILoggingService.LogEvents"/>), but you can do one/single entry, or many (plural)
    /// </remarks>
    public abstract Task<int> LogEvents(params LogEntry[] entries);

    /// <summary>
    /// Deletes all provided <see cref="LogEntry"/>-entries.
    /// </summary>

    public abstract Task<int> DeleteEvents(params LogEntry[] entries);
}