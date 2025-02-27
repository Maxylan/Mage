
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models;
using Reception.Interfaces;

namespace Reception.Services;

public class SessionService : ISessionService
{
    private readonly ILoggingService logging;
    private readonly MageDbContext db;

    public SessionService(ILoggingService loggingService, MageDbContext context)
    {
        logging = loggingService;
        db = context;
    }

    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Session&gt;"/>) set of 
    /// <see cref="Session"/>-entries, you may use it to freely fetch some sessions.
    /// </summary>
    public DbSet<Session> GetSessions() => db.Sessions;

    /// <summary>
    /// Get the <see cref="Session"/> with matching '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult<Session?>> GetSession(string code)
    {
        Session? session = await db.Sessions.FirstOrDefaultAsync(s => s.Code == code);

        if (session is null)
        {
            string message = $"Failed to find a {nameof(Session)} matching the given '{nameof(code)}'.";
            await logging.ExternalDebug(message);
            return new NotFoundObjectResult(message);
        }

        return session;
    }
    /// <summary>
    /// Get the <see cref="Session"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<Session?>> GetSessionById(int id)
    {
        Session? session = await db.Sessions.FindAsync(id);

        if (session is null)
        {
            string message = $"Failed to find a {nameof(Session)} with ID #{id}.";
            await logging.ExternalDebug(message);
            return new NotFoundObjectResult(message);
        }

        return session;
    }
    /// <summary>
    /// Get the current <see cref="Session"/> of the <see cref="Account"/> with Primary Key '<paramref ref="userId"/>'
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    public async Task<ActionResult<Session?>> GetSessionByUserId(int userId, bool deleteDuplicates = false)
    {
        Account? account = await db.Accounts
            .Include(account => account.Sessions)
            .FirstOrDefaultAsync(account => account.Id == userId);

        if (account is null)
        {
            string message = $"Failed to find an {nameof(Account)} with UID (PK) #{userId}.";
            await logging.ExternalDebug(message);
            return new NotFoundObjectResult(message);
        }

        if (account.Sessions is null || account.Sessions.Count == 0)
        {
            string message = $"{nameof(Account)} with UID (PK) #{userId} have no active/stored {nameof(Session)} instance.";
            await logging.ExternalDebug(message);
            return new NotFoundObjectResult(message);
        }
        else if (account.Sessions.Count > 1)
        {
            account.Sessions = account.Sessions
                .OrderByDescending(session => session.CreatedAt)
                .ToArray();
        
            if (deleteDuplicates) {
                for (int i = 1; i < account.Sessions.Count; i++) {
                    db.Remove(account.Sessions.ElementAt(i));
                }
            }
        }

        return account.Sessions.First();
    }
    /// <summary>
    /// Get the current <see cref="Session"/> of the <see cref="Account"/> with unique '<paramref ref="userName"/>'
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    public async Task<ActionResult<Session?>> GetSessionByUsername(int userName, bool deleteDuplicates = false)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Get the current <see cref="Session"/> of the given '<see cref="Account"/>'.
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    public async Task<ActionResult<Session?>> GetSessionByUser(Account user, bool deleteDuplicates = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete expired sessions &amp; duplicates from the database.
    /// </summary>
    /// <remarks>
    /// Duplicates = <i>Instances where a single user has more than one active session at the same time.</i>
    /// </remarks>
    public async Task<int> CleanupSessions()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete expired sessions from the database.
    /// </summary>
    public async Task<int> DeleteExpired()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete "duplicate" sessions from the database.
    /// </summary>
    /// <remarks>
    /// <i>(i.e, instances where 1 user has more than a single active session)</i>
    /// </remarks>
    public async Task<int> DeleteDuplicates()
    {
        throw new NotImplementedException();
    }
}