using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Reception.Interfaces;
using Reception.Models;
using Reception.Authentication;

namespace Reception.Services;

public class LoggingService(
    IHttpContextAccessor contextAccessor,
    ILogger<LoggingService> logger
) : ILoggingService
{
    private string nextAction = string.Empty;

    /// <summary>
    /// Get the current user's <see cref="Account"/> from the '<see cref="HttpContext"/>'
    /// </summary>
    /// <remarks>
    /// Catches most errors thrown, logs them, and finally returns `null`.
    /// </remarks>
    private Account? GetAccount()
    {
        if (!MageAuthentication.IsAuthenticated(contextAccessor))
        {
            if (Program.IsDevelopment)
            {
                logger.LogTrace($"{nameof(LoggingService.GetAccount)} called on an unauthorized request.");
            }

            return null;
        }

        try
        {
            return MageAuthentication.GetAccount(contextAccessor);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Cought an '{ex.GetType().FullName}' invoking {nameof(LoggingService.GetAccount)}!", ex.StackTrace);
            return null;
        }
    }

    /// <summary>
    /// Get the <see cref="ILogger{T}"/> used by this <see cref="ILoggingService"/>
    /// </summary>
    public ILogger Logger => logger;

    /// <summary>
    /// Get the <see cref="LogEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<LogEntry>> GetEvent(int id)
    {
        throw new NotImplementedException("TODO - Seems this is not as straight forward as I first thought.\rPossible solution would be a seperate DbContext for logging that's a singleton, perhaps?\rhttps://go.microsoft.com/fwlink/?linkid=869049");
        /* LogEntry? result = await db.Logs.FindAsync(id);
        if (result is null)
        {
            return new NotFoundObjectResult($"Failed to find {nameof(LogEntry)} with ID {id}");
        }

        return result; */
    }

    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;LogEntry&gt;"/>) set of
    /// <see cref="LogEntry"/>-entries, you may use it to freely fetch some logs.
    /// </summary>
    public DbSet<LogEntry> GetEvents() {
        throw new NotImplementedException("TODO - Seems this is not as straight forward as I first thought.\rPossible solution would be a seperate DbContext for logging that's a singleton, perhaps?\rhttps://go.microsoft.com/fwlink/?linkid=869049");
        /* return db.Logs; */
    }

    /// <summary>
    /// Get all <see cref="LogEntry"/>-entries matching a wide range of optional filtering parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<LogEntry>>> GetEvents(int? limit, int? offset, Source? source, Severity? severity, Method? method, string? action)
    {
        throw new NotImplementedException("TODO - Seems this is not as straight forward as I first thought.\rPossible solution would be a seperate DbContext for logging that's a singleton, perhaps?\rhttps://go.microsoft.com/fwlink/?linkid=869049");
        /* IQueryable<LogEntry> query = db.Logs.OrderByDescending(log => log.CreatedAt);
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
                logger.LogWarning($"[{nameof(LoggingService)}] ({nameof(GetEvents)}) {message}");

                return new BadRequestObjectResult(message);
            }

            query = query.Skip(offset.Value);
        }

        if (limit is not null)
        {
            if (limit <= 0)
            {
                message = $"Parameter {nameof(limit)} has to be a positive integer greater-than `0`.";
                logger.LogWarning($"[{nameof(LoggingService)}] ({nameof(GetEvents)}) {message}");

                return new BadRequestObjectResult(message);
            }

            query = query.Take(limit.Value);
        }

        var getLogs = await query.ToArrayAsync();
        return getLogs; */
    }

    #region Create Logs (w/ many shortcuts)
    /// <summary>
    /// Set what action triggered this entry to be created.
    /// Will be used for the next <see cref="LogEntry"/> created via <see cref="LogEvent"/>.
    /// </summary>
    public ILoggingService Action(string actionName)
    {
        this.nextAction = actionName;
        return this;
    }

    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogTrace(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.LogLevel = Severity.TRACE;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase InternalTrace(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.INTERNAL;
            entry.LogLevel = Severity.TRACE;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase ExternalTrace(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.EXTERNAL;
            entry.LogLevel = Severity.TRACE;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogDebug(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.LogLevel = Severity.DEBUG;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase InternalDebug(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.INTERNAL;
            entry.LogLevel = Severity.DEBUG;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase ExternalDebug(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.EXTERNAL;
            entry.LogLevel = Severity.DEBUG;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogInformation(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.LogLevel = Severity.INFORMATION;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase InternalInformation(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.INTERNAL;
            entry.LogLevel = Severity.INFORMATION;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase ExternalInformation(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.EXTERNAL;
            entry.LogLevel = Severity.INFORMATION;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.LogLevel = Severity.SUSPICIOUS;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase InternalSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.INTERNAL;
            entry.LogLevel = Severity.SUSPICIOUS;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase ExternalSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.EXTERNAL;
            entry.LogLevel = Severity.SUSPICIOUS;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogWarning(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.LogLevel = Severity.WARNING;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase InternalWarning(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.INTERNAL;
            entry.LogLevel = Severity.WARNING;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase ExternalWarning(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.EXTERNAL;
            entry.LogLevel = Severity.WARNING;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogError(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.LogLevel = Severity.ERROR;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase InternalError(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.INTERNAL;
            entry.LogLevel = Severity.ERROR;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase ExternalError(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.EXTERNAL;
            entry.LogLevel = Severity.ERROR;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogCritical(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.LogLevel = Severity.CRITICAL;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase InternalCritical(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.INTERNAL;
            entry.LogLevel = Severity.CRITICAL;
        });
    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase ExternalCritical(string message, Action<LogEntryOptions>? predicate = null) =>
        LogEvent(message, entry =>
        {
            if (predicate is not null)
            {
                predicate(entry);
            }
            entry.Source = Source.EXTERNAL;
            entry.LogLevel = Severity.CRITICAL;
        });

    /// <summary>
    /// Log a custom <see cref="LogEntry"/>-event to the database.
    /// </summary>
    public StoreLogsInDatabase LogEvent(string message, Action<LogEntryOptions>? predicate = null)
    {
        LogEntryOptions entry = new()
        {
            Log = message,
            CreatedAt = DateTime.UtcNow,
            Action = this.nextAction
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

        this.nextAction = string.Empty;

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
            /* bool isNew = db.Entry(entry).State == EntityState.Detached;
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
            } */ 

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
    #endregion

    /// <summary>
    /// Deletes all provided <see cref="LogEntry"/>-entries.
    /// </summary>
    public Task<int> DeleteEvents(params LogEntry[] entries)
    {
        throw new NotImplementedException("TODO - Seems this is not as straight forward as I first thought.\rPossible solution would be a seperate DbContext for logging that's a singleton, perhaps?\rhttps://go.microsoft.com/fwlink/?linkid=869049");
        // db.RemoveRange(entries);

        /* foreach(var entry in entries)
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
        } */

        // return await db.SaveChangesAsync();
    }
}
