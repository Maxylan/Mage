using Reception.Database;
using Reception.Database.Models;

namespace Reception.Middleware;

public class EventDataAggregator
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly IServiceScopeFactory _scopeFactory;

    // AsyncLocal ensures each async control flow (request) has its own list
    private static readonly AsyncLocal<Queue<LogEntry>?> _eventQueue = new();

    public EventDataAggregator(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Opens the  recording / 'Enqueue' a single event/log entry (<see cref="LogEntry"/>) to the aggregator
    /// This will be stored at the end of a request's lifecycle.
    /// </summary>
    public void BeginRequest()
    {
        _eventQueue.Value = new Queue<LogEntry>();
    }

    /// <summary>
    /// Record / 'Enqueue' a single event/log entry (<see cref="LogEntry"/>) to the aggregator
    /// This will be stored at the end of a request's lifecycle.
    /// </summary>
    public void AddEvent(LogEntry evt)
    {
        if (_eventQueue.Value is null) {
            this.BeginRequest();
        }

        _eventQueue.Value?.Enqueue(evt);
    }

    // Called at end of request to persist collected events
    public async Task EndRequestAsync()
    {
        var events = _eventQueue.Value;
        if (events is null || events.Count == 0) {
            return;
        }

        await this._semaphore.WaitAsync();
        try
        {
            // Create a new DI scope to resolve scoped services (e.g. DbContext)
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MageDb>();
            db.Logs.AddRange(events);

            await db.SaveChangesAsync();

            // Clear for this context
            _eventQueue.Value = null;
        }
        finally {
            this._semaphore.Release();
        }
    }
}
