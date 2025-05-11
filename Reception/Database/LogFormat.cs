using System.Text;
using Reception.Database.Models;

namespace Reception.Database;

public readonly struct LogFormat(LogEntry entry)
{
    private readonly LogEntry entry = entry;

    public string GetRequestDetails() => $"({(entry.RequestAddress ?? "Unknown/Hidden Address")}, {(entry.RequestUserAgent ?? "No UserAgent")})";
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
        if (string.IsNullOrWhiteSpace(userName))
        {
            return string.Empty;
        }
        if (entry.UserId is not null)
        {
            userName += $" (UID #{entry.UserId})";
        }

        return "by " + userName;
    }

    public string GetTitle(bool includeUser = false)
    {
        if (includeUser &&
            GetUser() is string user &&
            !string.IsNullOrWhiteSpace(user)
        )
        {
            return $"{entry.Action} {user} ->";
        }

        return $"{entry.Action} ->";
    }

    public string Short(bool includeTime = true)
    {
        StringBuilder sb = new();
        if (includeTime)
        {
            sb.Append(GetTime() + " ");
        }

        sb.AppendJoin(
            ' ',
            GetSeverity(),
            GetTitle(false),
            entry.Message
        );

        return sb.ToString();
    }

    public string Standard(bool includeUser = true)
    {
        StringBuilder sb = new();
        sb.AppendJoin(
            ' ',
            GetTime(),
            GetSeverity(),
            GetSource(),
            GetTitle(includeUser),
            entry.Message
        );

        return sb.ToString();
    }

    public string Full()
    {
        StringBuilder sb = new();
        sb.AppendJoin(
            ' ',
            GetTime(),
            GetSeverity(),
            GetSource(),
            GetTitle(true),
            GetRequestDetails(),
            entry.Message
        );

        return sb.ToString();
    }
}
