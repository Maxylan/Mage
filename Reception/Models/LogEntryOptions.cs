using Reception.Database.Models;

namespace Reception.Models;

public class LogEntryOptions : LogEntry
{
    public Exception? Exception { get; set; }

    /// <summary>
    /// Attempt to set user-related logging parameters.
    /// </summary>
    public void SetUser(Account? user)
    {
        if (user is null) {
            return;
        }

        this.UserId = user.Id;
        this.UserEmail = user.Email;
        this.UserUsername = user.Username;
        this.UserFullName = user.FullName;
    }
}
