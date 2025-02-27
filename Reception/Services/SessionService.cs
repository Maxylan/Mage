
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Reception.Interfaces;
using System.Net;

namespace Reception.Services;

public class SessionService(ILoggingService logging, MageDbContext db) : ISessionService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Session&gt;"/>) set of 
    /// <see cref="Session"/>-entries, you may use it to freely fetch some sessions.
    /// </summary>
    public DbSet<Session> GetSessions() => db.Sessions;

    /// <summary>
    /// Get the <see cref="Session"/> with matching '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult<Session>> GetSession(string code)
    {
        Session? session = await db.Sessions.FirstOrDefaultAsync(s => s.Code == code);

        if (session is null)
        {
            string message = $"Failed to find a {nameof(Session)} matching the given '{nameof(code)}'.";
            logging
                .Action(nameof(GetSession))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            string message = $"Failed to find a {nameof(Session)} matching the given '{nameof(code)}'.";
            logging
                .Action(nameof(GetSession))
                .ExternalDebug(message, m => m.Method = Method.GET);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return session;
    }
    /// <summary>
    /// Get the <see cref="Session"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<Session>> GetSessionById(int id)
    {
        Session? session = await db.Sessions.FindAsync(id);

        if (session is null)
        {
            string message = $"Failed to find a {nameof(Session)} with ID #{id}.";
            logging
                .Action(nameof(GetSessionById))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
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
    public async Task<ActionResult<Session>> GetSessionByUserId(int userId, bool deleteDuplicates = false)
    {
        Account? account = await db.Accounts
            .Include(account => account.Sessions)
            .FirstOrDefaultAsync(account => account.Id == userId);

        if (account is null)
        {
            string message = $"Failed to find an {nameof(Account)} with UID (PK) #{userId}.";
            logging
                .Action(nameof(GetSessionByUserId))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (account.Sessions is null || account.Sessions.Count == 0)
        {
            string message = $"{nameof(Account)} with UID (PK) #{userId} have no active/stored {nameof(Session)} instances.";
            logging
                .Action(nameof(GetSessionByUserId))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }
        else if (account.Sessions.Count > 1)
        {
            account.Sessions = account.Sessions
                .OrderByDescending(session => session.CreatedAt)
                .ToArray();
        
            if (deleteDuplicates)
            {
                for (int i = 1; i < account.Sessions.Count; i++) {
                    db.Remove(account.Sessions.ElementAt(i));
                }

                await db.SaveChangesAsync();
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
    public async Task<ActionResult<Session>> GetSessionByUsername(string userName, bool deleteDuplicates = false)
    {
        Account? account = await db.Accounts
            .Include(account => account.Sessions)
            .FirstOrDefaultAsync(account => account.Username == userName);

        if (account is null)
        {
            string message = $"Failed to find an {nameof(Account)} with Username '{userName}'.";
            logging
                .Action(nameof(GetSessionByUsername))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (account.Sessions is null || account.Sessions.Count == 0)
        {
            string message = $"{nameof(Account)} with Username '{userName}' have no active/stored {nameof(Session)} instances.";
            logging
                .Action(nameof(GetSessionByUsername))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }
        else if (account.Sessions.Count > 1)
        {
            account.Sessions = account.Sessions
                .OrderByDescending(session => session.CreatedAt)
                .ToArray();
        
            if (deleteDuplicates)
            {
                for (int i = 1; i < account.Sessions.Count; i++) {
                    db.Remove(account.Sessions.ElementAt(i));
                }

                await db.SaveChangesAsync();
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
    public async Task<ActionResult<Session>> GetSessionByUser(Account account, bool deleteDuplicates = false)
    {
        if (account.Sessions is not null && account.Sessions.Count > 0)
        {
            if (account.Sessions.Count > 1)
            {
                account.Sessions = account.Sessions
                    .OrderByDescending(session => session.CreatedAt)
                    .ToArray();
            
                if (deleteDuplicates)
                {
                    for (int i = 1; i < account.Sessions.Count; i++) {
                        db.Remove(account.Sessions.ElementAt(i));
                    }

                    await db.SaveChangesAsync();
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

        var sessions = await db.Sessions
            .Where(session => session.UserId == account.Id)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync();
        
        if (sessions is null || sessions.Count == 0)
        {
            string message = $"{nameof(Account)} '{account.Username}' (UID #{account.Id}) have no active/stored {nameof(Session)} instances.";
            logging
                .Action(nameof(GetSessionByUser))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }
        else if (sessions.Count > 1 && deleteDuplicates)
        {
            for (int i = 1; i < sessions.Count; i++) {
                db.Remove(sessions.ElementAt(i));
            }

            await db.SaveChangesAsync();
        }

        return sessions.First();
    }

    /// <summary>
    /// Get the <see cref="Account"/> associated with a given '<see cref="Session"/>'.
    /// </summary>
    public async Task<ActionResult<Account>> GetUserBySession(Session session)
    {
        Account? user = session.User;
        if (user is not null)
        {
            bool exists = await db.Accounts.ContainsAsync(user);
            
            if (exists) {
                return user;
            }
        }

        user = await db.Accounts.FindAsync(session.UserId);
        
        if (user is null)
        {
            string message = $"Failed to find an {nameof(Account)} matching the given {nameof(Session)}'s {nameof(Session.UserId)} #{session.UserId}.";
            logging
                .Action(nameof(GetSession))
                .ExternalDebug(message);
            
            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return user;
    }

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
    public async Task<ActionResult<Session>> CreateSession(Account account, HttpRequest? request = null, Source source = Source.INTERNAL)
    {
        string? userAgentHeader = null;
        if (request is not null) {
            userAgentHeader = request.Headers.UserAgent.ToString();
        }

        Session newSession = new() {
            UserId = account.Id,
            Code = Guid.NewGuid().ToString("D"),
            UserAgent = userAgentHeader,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow + (
                string.IsNullOrWhiteSpace(userAgentHeader)
                    ? TimeSpan.FromHours(1)
                    : TimeSpan.FromDays(1)
            )
        };

        db.Add(newSession);
        account.LastVisit = DateTime.UtcNow;

        string message = $"Created new {nameof(Session)} '{newSession.Code}' for user '{account.Username}' (#{account.Id})";
        if (source == Source.EXTERNAL)
        {
            logging
                .LogTrace(message, m => {
                    m.Action = nameof(CreateSession);
                    m.Source = source;
                });
        }
        else {
            logging.GetLogger().LogTrace(message);
        }

        int rowsInserted = await db.SaveChangesAsync();

        return rowsInserted > 0 && newSession.Id != default
            ? new OkObjectResult(newSession)
            : new NoContentResult();
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

        return await db.SaveChangesAsync();
    }

    /// <summary>
    /// Delete expired sessions from the database.
    /// </summary>
    public async Task<int> DeleteExpired()
    {
        var sessions = await db.Sessions
            .Where(session => session.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        if (sessions is not null && sessions.Count > 0)
        {
            db.RemoveRange(sessions);

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
        var sessions = await db.Sessions
            .Where(session => session.ExpiresAt <= DateTime.UtcNow)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync();

        sessions = sessions
            .Where((session, index) => sessions.IndexOf(session) != index)
            .ToList();

        if (sessions is not null && sessions.Count > 0)
        {
            db.RemoveRange(sessions);

            logging
                .Action(nameof(DeleteDuplicates))
                .InternalTrace($"Removing {sessions.Count} duplicate user sessions..");
            
            return sessions.Count;
        }

        return default;
    }
}