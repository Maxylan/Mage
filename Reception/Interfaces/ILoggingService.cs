
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Reception.Models;

namespace Reception.Interfaces;

public interface ILoggingService
{
    /// <summary>
    /// Get the <see cref="ILogger{T}"/> used by this <see cref="ILoggingService"/>
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Get the <see cref="LogEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<LogEntry>> GetEvent(int id);

    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;LogEntry&gt;"/>) set of 
    /// <see cref="LogEntry"/>-entries, you may use it to freely fetch some logs.
    /// </summary>
    public abstract DbSet<LogEntry> GetEvents();

    /// <summary>
    /// Get all <see cref="LogEntry"/>-entries matching a wide range of optional filtering parameters.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<LogEntry>>> GetEvents(int? limit, int? offset, Source? source, Severity? severity, Method? method, string? action);

    /// <summary>
    /// Set what action triggered this entry to be created.
    /// Will be used for the next <see cref="LogEntry"/> created via <see cref="LogNewEvent"/>.
    /// </summary>
    public abstract ILoggingService Action(string actionName);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogTrace(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalTrace(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalTrace(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogDebug(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalDebug(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalDebug(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogInformation(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalInformation(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalInformation(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogSuspicious(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalSuspicious(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalSuspicious(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogWarning(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalWarning(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalWarning(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogError(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalError(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalError(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogCritical(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalCritical(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalCritical(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogEvent(string message, Action<LogEntryOptions>? predicate = null);
    /// <summary>
    /// Log any number of custom <see cref="LogEntry"/>-events. Tracks entities as <see cref="EntityState.Added"/>,
    /// but does *<strong>not</strong>* call <see cref="DbContext.SaveChangesAsync"/>.
    /// </summary>
    public abstract StoreLogsInDatabase LogEvents(params LogEntryOptions[] entries);

    /// <summary>
    /// Deletes all provided <see cref="LogEntry"/>-entries.
    /// </summary>

    public abstract Task<int> DeleteEvents(params LogEntry[] entries);
}