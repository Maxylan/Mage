
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
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
    /// Set what action triggered this entry to be created.
    /// Will be used for the next <see cref="LogEntry"/> created via <see cref="LogNewEvent"/>.
    /// </summary>
    public abstract ILoggingService Action(string actionName);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogTrace(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalTrace(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalTrace(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogDebug(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalDebug(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalDebug(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogInformation(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalInformation(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalInformation(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogSuspicious(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalSuspicious(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalSuspicious(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogWarning(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalWarning(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalWarning(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogError(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalError(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalError(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogCritical(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase InternalCritical(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase ExternalCritical(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public abstract StoreLogsInDatabase LogEvent(string message, Action<LogEntry>? predicate = null);
    /// <summary>
    /// Log any number of custom <see cref="LogEntry"/>-events. Tracks entities as <see cref="EntityState.Added"/>,
    /// but does *<strong>not</strong>* call <see cref="DbContext.SaveChangesAsync"/>.
    /// </summary>
    public abstract StoreLogsInDatabase LogEvents(params LogEntry[] entries);

    /// <summary>
    /// Deletes all provided <see cref="LogEntry"/>-entries.
    /// </summary>

    public abstract Task<int> DeleteEvents(params LogEntry[] entries);
}