using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database;
using Reception.Database.Models;

namespace Reception.Interfaces.DataAccess;

public interface ISessionService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Session&gt;"/>) set of
    /// <see cref="Session"/>-entries, you may use it to freely fetch some sessions.
    /// </summary>
    public abstract DbSet<Session> GetSessions();

    /// <summary>
    /// Get the <see cref="Session"/> with matching '<paramref ref="code"/>'
    /// </summary>
    public abstract Task<ActionResult<Session>> GetSession(string code);
    /// <summary>
    /// Get the <see cref="Session"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<Session>> GetSessionById(int id);
    /// <summary>
    /// Get the current <see cref="Session"/> of the <see cref="Account"/> with Primary Key '<paramref ref="userId"/>'
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    /// <param name="deleteDuplicates">
    /// Provide '<c>true</c>' to if you want to automatically clean-up duplicates / old sessions from the database.
    /// </param>
    public abstract Task<ActionResult<Session>> GetSessionByUserId(int userId, bool deleteDuplicates = false);
    /// <summary>
    /// Get the current <see cref="Session"/> of the <see cref="Account"/> with unique '<paramref ref="userName"/>'
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    /// <param name="deleteDuplicates">
    /// Provide '<c>true</c>' to if you want to automatically clean-up duplicates / old sessions from the database.
    /// </param>
    public abstract Task<ActionResult<Session>> GetSessionByUsername(string userName, bool deleteDuplicates = false);
    /// <summary>
    /// Get the current <see cref="Session"/> of the given '<see cref="Account"/>'.
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    /// <param name="deleteDuplicates">
    /// Provide '<c>true</c>' to if you want to automatically clean-up duplicates / old sessions from the database.
    /// </param>
    public abstract Task<ActionResult<Session>> GetSessionByUser(Account account, bool deleteDuplicates = false);

    /// <summary>
    /// Get the <see cref="Account"/> associated with a given '<see cref="Session"/>'.
    /// </summary>
    public abstract Task<ActionResult<Account>> GetUserBySession(Session session);

    /// <summary>
    /// Create a new <see cref="Session"/> for the given '<see cref="Account"/>'.
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<paramref ref="request"/>' if you want to include a UserAgent header.
    /// <br/>Returns a '<see cref="NoContentResult"/>' if nothing was created/added, but nothing failed.
    /// </remarks>
    /// <param name="request">
    /// Provide an '<see cref="HttpRequest"/>' if you want to include a UserAgent header in the session.
    /// This will in turn extend its expiry from 1h to 24h (1 day).
    /// </param>
    public abstract Task<ActionResult<Session>> CreateSession(Account account, HttpRequest? request = null, Source source = Source.INTERNAL);

    /// <summary>
    /// Delete expired sessions &amp; duplicates from the database.
    /// </summary>
    /// <remarks>
    /// Duplicates = <i>Instances where a single user has more than one active session at the same time.</i>
    /// </remarks>
    public abstract Task<int> CleanupSessions();

    /// <summary>
    /// Delete expired sessions from the database.
    /// </summary>
    public abstract Task<int> DeleteExpired();

    /// <summary>
    /// Delete "duplicate" sessions from the database.
    /// </summary>
    /// <remarks>
    /// <i>(i.e, instances where 1 user has more than a single active session)</i>
    /// </remarks>
    public abstract Task<int> DeleteDuplicates();
}
