using Reception.Middleware;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Services {
    public class LoggingService(
        ILogger logger,
        EventDataAggregator eventAggregator
    ) : ILoggingService
    {
        /// <summary>
        /// Get the <see cref="ILogger{T}"/> used by this <see cref="ILoggingService{TService}"/>
        /// Use this to, for example, log a message without storing it in the database.
        /// </summary>
        public ILogger Logger => logger;

        protected LogEntryOptions? _log = null;

        #region Create Logs (w/ many shortcuts)
        /// <summary>
        /// Set what action triggered this entry to be created.
        /// Will be used for the next <see cref="LogEntry"/> created via <see cref="LogEvent"/>.
        /// </summary>
        public ILoggingService Action(string actionName)
        {
            if (this._log is null) {
                this._log = new();
            }

            this._log.Action = actionName;
            return this;
        }

        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService LogTrace(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.TRACE;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService InternalTrace(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService ExternalTrace(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService LogDebug(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.DEBUG;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService InternalDebug(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService ExternalDebug(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService LogInformation(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.INFORMATION;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService InternalInformation(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService ExternalInformation(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService LogSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.SUSPICIOUS;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService InternalSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService ExternalSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService LogWarning(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.WARNING;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService InternalWarning(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService ExternalWarning(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService LogError(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.ERROR;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService InternalError(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService ExternalError(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService LogCritical(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.CRITICAL;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService InternalCritical(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService ExternalCritical(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.Source = Source.EXTERNAL;
                entry.LogLevel = Severity.CRITICAL;
            });

        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event entry.
        /// </summary>
        protected ILoggingService StoreEvent(string message, Action<LogEntryOptions>? predicate = null) {
            this._log = new() {
                Message = message
            };

            if (predicate is not null) {
                predicate(this._log);
            }

            return this;
        }

        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event entry.
        /// </summary>
        public void Log() {
            if (this._log is null) {
                logger.LogWarning($"Method {nameof(LoggingService.Log)} used incorrectly! Called without any log ({nameof(LogEntryOptions)}) stored!");
                return;
            }

            if (string.IsNullOrWhiteSpace(this._log.Action))
            {
                this._log.Action = "Unknown";
            }

            switch (this._log.LogLevel)
            {
    #pragma warning disable CA2254
                case Severity.TRACE:
                    logger.LogTrace(this._log.Exception, this._log.Format.Short(false));
                    break;
                case Severity.DEBUG:
                    logger.LogDebug(this._log.Exception, this._log.Format.Short());
                    break;
                case Severity.INFORMATION:
                    logger.LogInformation(this._log.Exception, this._log.Format.Standard());
                    break;
                case Severity.SUSPICIOUS:
                    logger.LogWarning(this._log.Exception, this._log.Format.Standard());
                    break;
                case Severity.WARNING:
                    logger.LogWarning(this._log.Exception, this._log.Format.Standard());
                    break;
                case Severity.ERROR:
                    logger.LogError(this._log.Exception, this._log.Format.Full());
                    break;
                case Severity.CRITICAL:
                    logger.LogCritical(this._log.Exception, this._log.Format.Full());
                    break;
                default:
                    this._log.Message += $" ({nameof(LogEntry)} format defaulted)";
                    logger.LogInformation(this._log.Exception, this._log.Format.Short(true));
                    break;
    #pragma warning restore CA2254
            }
        }

        /// <summary>
        /// Store / "Enqueue" a custom <see cref="LogEntry"/>-event entry, which will be added to the database (..on request-lifecycle end).
        /// </summary>
        public void Enqueue() {
            if (this._log is not null) {
                if (string.IsNullOrWhiteSpace(this._log.Action))
                {
                    this._log.Action = "Unknown";
                }

                eventAggregator.AddEvent(this._log);
            }
        }

        /// <summary>
        /// Log & Store / "Enqueue" a custom <see cref="LogEntry"/>-event entry, which will be added to the database (..on request-lifecycle end).
        /// </summary>
        public void LogAndEnqueue() {
            this.Log();
            this.Enqueue();
        }
        #endregion
    }

    public class LoggingService<TService>(
        ILogger<TService> logger,
        EventDataAggregator eventAggregator
    ) : ILoggingService<TService>
    {
        /// <summary>
        /// Get the <see cref="ILogger{T}"/> used by this <see cref="ILoggingService{TService}"/>
        /// Use this to, for example, log a message without storing it in the database.
        /// </summary>
        public ILogger<TService> Logger => logger;

        protected LogEntryOptions? _log = null;

        #region Create Logs (w/ many shortcuts)
        /// <summary>
        /// Set what action triggered this entry to be created.
        /// Will be used for the next <see cref="LogEntry"/> created via <see cref="LogEvent"/>.
        /// </summary>
        public ILoggingService<TService> Action(string actionName)
        {
            if (this._log is null) {
                this._log = new();
            }

            this._log.Action = actionName;
            return this;
        }

        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> LogTrace(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.TRACE;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> InternalTrace(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> ExternalTrace(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> LogDebug(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.DEBUG;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> InternalDebug(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> ExternalDebug(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> LogInformation(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.INFORMATION;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> InternalInformation(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> ExternalInformation(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> LogSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.SUSPICIOUS;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> InternalSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> ExternalSuspicious(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> LogWarning(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.WARNING;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> InternalWarning(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> ExternalWarning(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> LogError(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.ERROR;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> InternalError(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> ExternalError(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> LogCritical(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.LogLevel = Severity.CRITICAL;
            });
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> InternalCritical(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
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
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public ILoggingService<TService> ExternalCritical(string message, Action<LogEntryOptions>? predicate = null) =>
            StoreEvent(message, entry =>
            {
                if (predicate is not null)
                {
                    predicate(entry);
                }
                entry.Source = Source.EXTERNAL;
                entry.LogLevel = Severity.CRITICAL;
            });

        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event entry.
        /// </summary>
        protected ILoggingService<TService> StoreEvent(string message, Action<LogEntryOptions>? predicate = null) {
            this._log = new() {
                Message = message
            };

            if (predicate is not null) {
                predicate(this._log);
            }

            return this;
        }

        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event entry.
        /// </summary>
        public void Log() {
            if (this._log is null) {
                logger.LogWarning($"Method {nameof(LoggingService<TService>.Log)} used incorrectly! Called without any log ({nameof(LogEntryOptions)}) stored!");
                return;
            }

            if (string.IsNullOrWhiteSpace(this._log.Action))
            {
                this._log.Action = "Unknown";
            }

            switch (this._log.LogLevel)
            {
    #pragma warning disable CA2254
                case Severity.TRACE:
                    logger.LogTrace(this._log.Exception, this._log.Format.Short(false));
                    break;
                case Severity.DEBUG:
                    logger.LogDebug(this._log.Exception, this._log.Format.Short());
                    break;
                case Severity.INFORMATION:
                    logger.LogInformation(this._log.Exception, this._log.Format.Standard());
                    break;
                case Severity.SUSPICIOUS:
                    logger.LogWarning(this._log.Exception, this._log.Format.Standard());
                    break;
                case Severity.WARNING:
                    logger.LogWarning(this._log.Exception, this._log.Format.Standard());
                    break;
                case Severity.ERROR:
                    logger.LogError(this._log.Exception, this._log.Format.Full());
                    break;
                case Severity.CRITICAL:
                    logger.LogCritical(this._log.Exception, this._log.Format.Full());
                    break;
                default:
                    this._log.Message += $" ({nameof(LogEntry)} format defaulted)";
                    logger.LogInformation(this._log.Exception, this._log.Format.Short(true));
                    break;
    #pragma warning restore CA2254
            }
        }

        /// <summary>
        /// Store / "Enqueue" a custom <see cref="LogEntry"/>-event entry, which will be added to the database (..on request-lifecycle end).
        /// </summary>
        public void Enqueue() {
            if (this._log is not null) {
                if (string.IsNullOrWhiteSpace(this._log.Action))
                {
                    this._log.Action = "Unknown";
                }

                eventAggregator.AddEvent(this._log);
            }
        }

        /// <summary>
        /// Log & Store / "Enqueue" a custom <see cref="LogEntry"/>-event entry, which will be added to the database (..on request-lifecycle end).
        /// </summary>
        public void LogAndEnqueue() {
            this.Log();
            this.Enqueue();
        }
        #endregion
    }
}
