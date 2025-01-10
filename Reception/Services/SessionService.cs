
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Reception.Interfaces;

namespace Reception.Services;

public class SessionService(ILoggingService logging, MageDbContext context) : ISessionService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Session&gt;"/>) set of 
    /// <see cref="Session"/>-entries, you may use it to freely fetch some sessions.
    /// </summary>
    public DbSet<Session> GetSessions() => context.Sessions;

    /// <summary>
    /// Get the <see cref="Session"/> with matching '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult<Session?>> GetSession(string code)
    {
        Session? session = await context.Sessions.FirstOrDefaultAsync(s => s.Code == code);

        if (session is null)
        {
            string message = $"Failed to find a {nameof(Session)} matching the given '{nameof(code)}'.";
            logging
                .Action(nameof(GetSession))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(message);
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            string message = $"Failed to find a {nameof(Session)} matching the given '{nameof(code)}'.";
            logging
                .Action(nameof(GetSession))
                .ExternalDebug(message, m => m.Method = Method.GET);
            
            return new NotFoundObjectResult(message);
        }

        return session;
    }
    /// <summary>
    /// Get the <see cref="Session"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<Session?>> GetSessionById(int id)
    {
        Session? session = await context.Sessions.FindAsync(id);

        if (session is null)
        {
            string message = $"Failed to find a {nameof(Session)} with ID #{id}.";
            logging
                .Action(nameof(GetSessionById))
                .ExternalDebug(message);
            
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
    /// <param name="deleteDuplicates">
    /// Provide '<c>true</c>' to if you want to automatically clean-up duplicates / old sessions from the database.
    /// </param>
    public async Task<ActionResult<Session?>> GetSessionByUserId(int userId, bool deleteDuplicates = false)
    {
        Account? account = await context.Accounts
            .Include(account => account.Sessions)
            .FirstOrDefaultAsync(account => account.Id == userId);

        if (account is null)
        {
            string message = $"Failed to find an {nameof(Account)} with UID (PK) #{userId}.";
            logging
                .Action(nameof(GetSessionByUserId))
                .ExternalDebug(message)
                .Save();
            
            return new NotFoundObjectResult(message);
        }

        if (account.Sessions is null || account.Sessions.Count == 0)
        {
            string message = $"{nameof(Account)} with UID (PK) #{userId} have no active/stored {nameof(Session)} instances.";
            logging
                .Action(nameof(GetSessionByUserId))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(message);
        }
        else if (account.Sessions.Count > 1)
        {
            account.Sessions = account.Sessions
                .OrderByDescending(session => session.CreatedAt)
                .ToArray();
        
            if (deleteDuplicates) {
                for (int i = 1; i < account.Sessions.Count; i++) {
                    context.Remove(account.Sessions.ElementAt(i));
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
    /// <param name="deleteDuplicates">
    /// Provide '<c>true</c>' to if you want to automatically clean-up duplicates / old sessions from the database.
    /// </param>
    public async Task<ActionResult<Session?>> GetSessionByUsername(string userName, bool deleteDuplicates = false)
    {
        Account? account = await context.Accounts
            .Include(account => account.Sessions)
            .FirstOrDefaultAsync(account => account.Username == userName);

        if (account is null)
        {
            string message = $"Failed to find an {nameof(Account)} with Username '{userName}'.";
            logging
                .Action(nameof(GetSessionByUsername))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(message);
        }

        if (account.Sessions is null || account.Sessions.Count == 0)
        {
            string message = $"{nameof(Account)} with Username '{userName}' have no active/stored {nameof(Session)} instances.";
            logging
                .Action(nameof(GetSessionByUsername))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(message);
        }
        else if (account.Sessions.Count > 1)
        {
            account.Sessions = account.Sessions
                .OrderByDescending(session => session.CreatedAt)
                .ToArray();
        
            if (deleteDuplicates) {
                for (int i = 1; i < account.Sessions.Count; i++) {
                    context.Remove(account.Sessions.ElementAt(i));
                }
            }
        }

        return account.Sessions.First();
    }
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
    public async Task<ActionResult<Session?>> GetSessionByUser(Account account, bool deleteDuplicates = false)
    {
        if (account.Sessions is not null && account.Sessions.Count > 0)
        {
            if (account.Sessions.Count > 1)
            {
                account.Sessions = account.Sessions
                    .OrderByDescending(session => session.CreatedAt)
                    .ToArray();
            
                if (deleteDuplicates) {
                    for (int i = 1; i < account.Sessions.Count; i++) {
                        context.Remove(account.Sessions.ElementAt(i));
                    }
                }
            }

            var session = account.Sessions.First();

            // Use it only if its valid, if its not we want to verify that we have the latest 
            // sessions with a call to the database.
            if (session.ExpiresAt > DateTime.UtcNow)
            {
                return session;
            }
        }

        var sessions = await context.Sessions
            .Where(session => session.UserId == account.Id)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync();
        
        if (sessions is null || sessions.Count == 0)
        {
            string message = $"{nameof(Account)} '{account.Username}' (UID #{account.Id}) have no active/stored {nameof(Session)} instances.";
            logging
                .Action(nameof(GetSessionByUsername))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(message);
        }
        else if (sessions.Count > 1 && deleteDuplicates)
        {
            for (int i = 1; i < sessions.Count; i++) {
                context.Remove(sessions.ElementAt(i));
            }
        }

        return sessions.First();
    }

    /// <summary>
    /// Delete expired sessions &amp; duplicates from the database.
    /// </summary>
    /// <remarks>
    /// Duplicates = <i>Instances where a single user has more than one active session at the same time.</i>
    /// </remarks>
    public async Task<int> CleanupSessions()
    {
        await DeleteExpired();
        await DeleteDuplicates();

        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Delete expired sessions from the database.
    /// </summary>
    public async Task<int> DeleteExpired()
    {
        var sessions = await context.Sessions
            .Where(session => session.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        if (sessions is not null && sessions.Count > 0)
        {
            context.RemoveRange(sessions);

            logging
                .Action(nameof(DeleteExpired))
                .InternalTrace($"Removing {sessions.Count} expired sessions..");
            
            return sessions.Count;
        }

        return default;
    }

    /// <summary>
    /// Delete "duplicate" sessions from the database.
    /// </summary>
    /// <remarks>
    /// <i>(i.e, instances where 1 user has more than a single active session)</i>
    /// </remarks>
    public async Task<int> DeleteDuplicates()
    {
        var sessions = await context.Sessions
            .Where(session => session.ExpiresAt <= DateTime.UtcNow)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync();

        sessions = sessions
            .Where((session, index) => sessions.IndexOf(session) != index)
            .ToList();

        if (sessions is not null && sessions.Count > 0)
        {
            context.RemoveRange(sessions);

            logging
                .Action(nameof(DeleteDuplicates))
                .InternalTrace($"Removing {sessions.Count} duplicate user sessions..");
            
            return sessions.Count;
        }

        return default;
    }
}