using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces {
    public interface ILoggingService
    {
        /// <summary>
        /// Get the <see cref="ILogger{T}"/> used by this <see cref="ILoggingService"/>
        /// Use this to, for example, log a message without storing it in the database.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Set what action triggered this log entry to be created.
        /// </summary>
        public abstract ILoggingService Action(string actionName);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService LogTrace(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService InternalTrace(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService ExternalTrace(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService LogDebug(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService InternalDebug(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService ExternalDebug(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService LogInformation(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService InternalInformation(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService ExternalInformation(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService LogSuspicious(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService InternalSuspicious(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService ExternalSuspicious(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService LogWarning(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService InternalWarning(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService ExternalWarning(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService LogError(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService InternalError(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService ExternalError(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService LogCritical(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService InternalCritical(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService ExternalCritical(string message, Action<LogEntryOptions>? predicate = null);

        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event entry.
        /// </summary>
        public abstract void Log();

        /// <summary>
        /// Store / "Enqueue" a custom <see cref="LogEntry"/>-event entry, which will be added to the database (..on request-lifecycle end).
        /// </summary>
        public abstract void Enqueue();

        /// <summary>
        /// Log & Store / "Enqueue" a custom <see cref="LogEntry"/>-event entry, which will be added to the database (..on request-lifecycle end).
        /// </summary>
        public abstract void LogAndEnqueue();
    }
    public interface ILoggingService<TService>
    {
        /// <summary>
        /// Get the <see cref="ILogger{T}"/> used by this <see cref="ILoggingService{TService}"/>
        /// Use this to, for example, log a message without storing it in the database.
        /// </summary>
        public ILogger<TService> Logger { get; }

        /// <summary>
        /// Set what action triggered this log entry to be created.
        /// </summary>
        public abstract ILoggingService<TService> Action(string actionName);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> LogTrace(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> InternalTrace(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> ExternalTrace(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> LogDebug(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> InternalDebug(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> ExternalDebug(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> LogInformation(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> InternalInformation(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> ExternalInformation(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> LogSuspicious(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> InternalSuspicious(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> ExternalSuspicious(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> LogWarning(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> InternalWarning(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> ExternalWarning(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> LogError(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> InternalError(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> ExternalError(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> LogCritical(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> InternalCritical(string message, Action<LogEntryOptions>? predicate = null);
        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event to the database.
        /// The log is enqueued, meaning nothing is addded to the database until *after* this current request lifecycle is concluded.
        /// </summary>
        public abstract ILoggingService<TService> ExternalCritical(string message, Action<LogEntryOptions>? predicate = null);

        /// <summary>
        /// Log a custom <see cref="LogEntry"/>-event entry.
        /// </summary>
        public abstract void Log();

        /// <summary>
        /// Store / "Enqueue" a custom <see cref="LogEntry"/>-event entry, which will be added to the database (..on request-lifecycle end).
        /// </summary>
        public abstract void Enqueue();

        /// <summary>
        /// Log & Store / "Enqueue" a custom <see cref="LogEntry"/>-event entry, which will be added to the database (..on request-lifecycle end).
        /// </summary>
        public abstract void LogAndEnqueue();
    }
}
