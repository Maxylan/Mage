using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Interfaces.DataAccess;
using Reception.Models;
using Reception.Authentication;

namespace Reception.Services.DataAccess;

public class EventLogService(
    MageDb db,
    IHttpContextAccessor contextAccessor,
    ILogger<EventLogService> logger
) : IEventLogService
{
    /// <summary>
    /// Get the <see cref="LogEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<LogEntry>> GetEventLog(int id)
    {
        LogEntry? result = await db.Logs.FindAsync(id);
        if (result is null)
        {
            return new NotFoundObjectResult($"Failed to find {nameof(LogEntry)} with ID {id}");
        }

        return result;
    }

    // throw new NotImplementedException("TODO - Seems this is not as straight forward as I first thought.\rPossible solution would be a seperate DbContext for logging that's a singleton, perhaps?\rhttps://go.microsoft.com/fwlink/?linkid=869049");
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;LogEntry&gt;"/>) set of
    /// <see cref="LogEntry"/>-entries, you may use it to freely fetch some logs.
    /// </summary>
    public DbSet<LogEntry> GetEvents() {
        return db.Logs;
    }

    // throw new NotImplementedException("TODO - Seems this is not as straight forward as I first thought.\rPossible solution would be a seperate DbContext for logging that's a singleton, perhaps?\rhttps://go.microsoft.com/fwlink/?linkid=869049");
    /// <summary>
    /// Get all <see cref="LogEntry"/>-entries matching a wide range of optional filtering parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<LogEntry>>> GetEventLogs(int? limit, int? offset, Source? source, Severity? severity, Method? method, string? action)
    {
        IQueryable<LogEntry> query = db.Logs.OrderByDescending(log => log.CreatedAt);
        string message;

        if (source is not null)
        {
            query = query.Where(log => log.Source == source);
        }
        if (severity is not null)
        {
            query = query.Where(log => log.LogLevel == severity);
        }
        if (method is not null)
        {
            query = query.Where(log => log.Method == method);
        }
        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(log => log.Action == action);
        }

        if (offset is not null)
        {
            if (offset < 0)
            {
                message = $"Parameter {nameof(offset)} has to either be `0`, or any positive integer greater-than `0`.";
                logger.LogWarning($"[{nameof(EventLogService)}] ({nameof(GetEvents)}) {message}");

                return new BadRequestObjectResult(message);
            }

            query = query.Skip(offset.Value);
        }

        if (limit is not null)
        {
            if (limit <= 0)
            {
                message = $"Parameter {nameof(limit)} has to be a positive integer greater-than `0`.";
                logger.LogWarning($"[{nameof(EventLogService)}] ({nameof(GetEvents)}) {message}");

                return new BadRequestObjectResult(message);
            }

            query = query.Take(limit.Value);
        }

        var getLogs = await query.ToArrayAsync();
        return getLogs;
    }

    /// <summary>
    /// Get the current user's <see cref="Account"/> from the '<see cref="HttpContext"/>'
    /// Get the <see cref="ILogger{T}"/> used by this <see cref="ILoggingService{TService}"/>
    /// Use this to, for example, log a message without storing it in the database.
    /// </summary>
    /// <remarks>
    /// Catches most errors thrown, logs them, and finally returns `null`.
    /// </remarks>
    protected Account? GetAccount()
    {
        if (!MageAuthentication.IsAuthenticated(contextAccessor))
        {
            if (Program.IsDevelopment)
            {
                logger.LogTrace($"{nameof(EventLogService.GetAccount)} called on an unauthorized request.");
            }

            return null;
        }

        try
        {
            return MageAuthentication.GetAccount(contextAccessor);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Cought an '{ex.GetType().FullName}' invoking {nameof(EventLogService.GetAccount)}!", ex.StackTrace);
            return null;
        }
    }

    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogEvent(string message, Action<LogEntryOptions>? predicate = null)
    {
        LogEntryOptions entry = new()
        {
            Log = message,
            CreatedAt = DateTime.UtcNow
        };

        if (contextAccessor.HttpContext is not null)
        {
            entry.SetMethod(contextAccessor.HttpContext.Request.Method);

            entry.RequestAddress = MageAuthentication.GetRemoteAddress(contextAccessor.HttpContext);
            entry.RequestUserAgent = contextAccessor.HttpContext.Request.Headers.UserAgent.ToString();

            if (MageAuthentication.IsAuthenticated(contextAccessor))
            {
                Account? user = GetAccount();
                if (user is not null)
                {
                    entry.UserId = user.Id;
                    entry.UserUsername = user.Username;
                    entry.UserFullName = user.FullName;
                    entry.UserEmail = user.Email;
                }
            }
        }

        if (predicate is not null)
        {
            predicate(entry);
        }

        if (string.IsNullOrWhiteSpace(entry.Action))
        {
            entry.Action = "Unknown";
        }

        return LogEvents(entry);
    }

    /// <summary>
    /// Log any number of custom <see cref="LogEntry"/>-events. Tracks entities as <see cref="EntityState.Added"/>,
    /// but does *<strong>not</strong>* call <see cref="DbContext.SaveChangesAsync"/>.
    /// </summary>
    public StoreLogsInDatabase LogEvents(params LogEntryOptions[] entries)
    {
        foreach (var entry in entries)
        {
            /* TODO -
             * Seems this is not as straight forward as I first thought.
             * Possible solution would be a seperate DbContext for logging that's a singleton, perhaps?
             * @see https://go.microsoft.com/fwlink/?linkid=869049
             */
            bool isNew = db.Entry(entry).State == EntityState.Detached;
            bool shouldStore = (
                entry.LogLevel != Severity.DEBUG ||
                Program.IsDevelopment
            );

            if (isNew && shouldStore)
            {
                switch (db.Logs.Contains(entry))
                {
                    case true: // Exists
                        db.Update(entry);
                        break;
                    case false: // New
                        db.Add(entry);
                        break;
                }
            }

            bool isUserAuthenticated = (
                contextAccessor.HttpContext is not null &&
                MageAuthentication.IsAuthenticated(contextAccessor.HttpContext!)
            );

            switch (entry.LogLevel)
            {
#pragma warning disable CA2254
                case Severity.TRACE:
                    logger.LogTrace(entry.Exception, entry.Format.Short(false));
                    break;
                case Severity.DEBUG:
                    logger.LogDebug(entry.Exception, entry.Format.Short());
                    break;
                case Severity.INFORMATION:
                    logger.LogInformation(entry.Exception, entry.Format.Standard(isUserAuthenticated));
                    break;
                case Severity.SUSPICIOUS:
                    logger.LogWarning(entry.Exception, entry.Format.Standard(true));
                    break;
                case Severity.WARNING:
                    logger.LogWarning(entry.Exception, entry.Format.Standard(isUserAuthenticated));
                    break;
                case Severity.ERROR:
                    logger.LogError(entry.Exception, entry.Format.Full());
                    break;
                case Severity.CRITICAL:
                    logger.LogCritical(entry.Exception, entry.Format.Full());
                    break;
                default:
                    entry.Log += $" ({nameof(LogEntry)} format defaulted)";
                    logger.LogInformation(entry.Exception, entry.Format.Short(true));
                    break;
#pragma warning restore CA2254
            }
        }

        return new(/*db*/);
    }

    // throw new NotImplementedException("TODO - Seems this is not as straight forward as I first thought.\rPossible solution would be a seperate DbContext for logging that's a singleton, perhaps?\rhttps://go.microsoft.com/fwlink/?linkid=869049");
    /// <summary>
    /// Deletes all provided <see cref="LogEntry"/>-entries.
    /// </summary>
    public async Task<int> DeleteEvents(params LogEntry[] entries)
    {
        // db.RemoveRange(entries);

        foreach(var entry in entries)
        {
            bool exists = await db.Logs.ContainsAsync(entry);

            if (exists)
            {
                db.Remove(entry);
            }
            else {
                // Stop tracking the entity, this should result in nothing being added on next save, effectively "deleting" the entity, without a database call.
                db.Entry(entry).State = EntityState.Detached;
            }
        }

        return await db.SaveChangesAsync();
    }
}
