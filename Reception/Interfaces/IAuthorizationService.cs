
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Reception.Interfaces;

public interface IAuthorizationService
{
    /// <summary>
    /// Get the <see cref="Session"/> with matching '<paramref ref="code"/>'
    /// </summary>
    public abstract Task<IStatusCodeActionResult> ValidateSession(HttpContext httpContext);
    /// <summary>
    /// Get the <see cref="Session"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<Session?>> GetSessionById(int id);
    /// <summary>
    /// Get the current <see cref="Session"/> of the <see cref="Account"/> with Primary Key '<paramref ref="userId"/>'
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    public abstract Task<ActionResult<Session?>> GetSessionByUserId(int userId, bool deleteDuplicates = false);
    /// <summary>
    /// Get the current <see cref="Session"/> of the <see cref="Account"/> with unique '<paramref ref="userName"/>'
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    public abstract Task<ActionResult<Session?>> GetSessionByUsername(int userName, bool deleteDuplicates = false);
    /// <summary>
    /// Get the current <see cref="Session"/> of the given '<see cref="Account"/>'.
    /// </summary>
    /// <remarks>
    /// You may optionally provide '<c>true</c>' to '<paramref ref="deleteDuplicates"/>' if you want to automatically
    /// clean-up duplicates / old sessions from the database.
    /// </remarks>
    public abstract Task<ActionResult<Session?>> GetSessionByUser(Account user, bool deleteDuplicates = false);

    /// <summary>
    /// Delete expired sessions & duplicates from the database.
    /// </summary>
    public abstract DbSet<Session> DeleteExpiredSessions();
}