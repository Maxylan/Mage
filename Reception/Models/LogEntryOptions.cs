using Reception.Models.Entities;

namespace Reception.Models;

public class LogEntryOptions : LogEntry
{
    public Exception? Exception { get; set; }
}
