using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Reception.Models;

namespace Reception.Interfaces;

public interface IEventLogService
{
    /// <summary>
    /// Get the <see cref="LogEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<LogEntry>> GetEventLog(int id);

    /// <summary>
    /// Get all <see cref="LogEntry"/>-entries matching a wide range of optional filtering parameters.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<LogEntry>>> GetEventLogs(int? limit, int? offset, Source? source, Severity? severity, Method? method, string? action);

    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;LogEntry&gt;"/>) set of
    /// <see cref="LogEntry"/>-entries, you may use it to freely fetch some logs.
    /// </summary>
    public abstract DbSet<LogEntry> GetEvents();

    /// <summary>
    /// Deletes all provided <see cref="LogEntry"/>-entries.
    /// </summary>
    public abstract Task<int> DeleteEvents(params LogEntry[] entries);
}
