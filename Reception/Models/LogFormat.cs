using System.Text;

namespace Reception.Models;

public readonly struct LogFormat(LogEntry entry)
{
    private readonly LogEntry entry = entry;

    public string GetTime() => $"[{entry.CreatedAt.ToShortTimeString()}]";
    public string GetSeverity() => $"[{entry.LogLevel.ToString()}]";
    public string GetSource() => $"({entry.Source.ToString()}) {entry.Method.ToString()}";
    public string GetUser() 
    {
        string? userName = (
            entry.UserFullName ?? 
            entry.UserEmail ?? 
            entry.UserUsername
        );
        if (string.IsNullOrWhiteSpace(userName)) {
            return string.Empty;
        }
        if (entry.UserId is not null) {
            userName += $" (UID #{entry.UserId})";
        }
        
        return "by " + userName;
    }

    public string GetTitle(bool includeUser = false)
    {
        if (includeUser &&
            GetUser() is string user &&
            !string.IsNullOrWhiteSpace(user)
        ) {
            return $"{entry.Action} {user} ->";
        }

        return $"{entry.Action} ->";
    }

    public string Short(bool includeTime = true, bool includeUser = false)
    {
        StringBuilder sb = new();
        if (includeTime) {
            sb.Append(GetTime() + " ");
        }

        sb.AppendJoin(
            " ",
            GetSeverity(),
            GetTitle(includeUser),
            entry.Log
        );

        return sb.ToString();
    }

    public string Standard(bool includeUser = true)
    {
        StringBuilder sb = new();
        sb.AppendJoin(
            " ",
            GetTime(),
            GetSeverity(),
            GetSource(),
            GetTitle(includeUser),
            entry.Log
        );

        return sb.ToString();
    }
}