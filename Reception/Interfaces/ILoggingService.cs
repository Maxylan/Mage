
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models;

namespace Reception.Interfaces;

public interface ILoggingService
{
    /// <summary>
    /// Get the <see cref="LogEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<LogEntry?>> GetEvent(uint id);
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;LogEntry&gt;"/>) set of 
    /// <see cref="LogEntry"/>-entries, you may use it to freely fetch some logs.
    /// </summary>
    public abstract DbSet<LogEntry> GetEvents();

    public abstract Task<ActionResult<LogEntry>> LogTrace(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> InternalTrace(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> ExternalTrace(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> LogDebug(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> InternalDebug(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> ExternalDebug(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> LogInformation(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> InternalInformation(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> ExternalInformation(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> LogSuspicious(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> InternalSuspicious(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> ExternalSuspicious(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> LogWarning(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> InternalWarning(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> ExternalWarning(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> LogError(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> InternalError(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> ExternalError(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> LogCritical(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> InternalCritical(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> ExternalCritical(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> LogEvent(Action<LogEntry> predicate);
    public abstract Task<ActionResult<LogEntry>> LogEvent(LogEntry entry);

    public abstract Task<uint> DeleteEvents(params LogEntry[] entries);
}