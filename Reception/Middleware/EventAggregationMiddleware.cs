namespace Reception.Middleware;

public class EventAggregationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EventDataAggregator _aggregator;

    public EventAggregationMiddleware(RequestDelegate next, EventDataAggregator aggregator)
    {
        _next = next;
        _aggregator = aggregator;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _aggregator.BeginRequest();

        try
        {
            await _next(context);
        }
        finally
        {
            // Ensure we flush data even if an exception occurred
            await _aggregator.EndRequestAsync();
        }
    }
}
