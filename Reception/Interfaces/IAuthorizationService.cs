
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Reception.Interfaces;

public interface IAuthorizationService
{
    /// <summary>
    /// Validates that a session (..inferred from `<see cref="HttpContext"/>`) ..exists and is valid.
    /// </summary>
    /// <remarks>
    /// Argument <paramref name="source"/> Assumes <see cref="Source.EXTERNAL"/> by-default
    /// </remarks>
    /// <param name="source">Assumes <see cref="Source.EXTERNAL"/> by-default</param>
    public abstract Task<ActionResult<Session>> ValidateSession(Source source = Source.EXTERNAL);
    /// <summary>
    /// Validates that a given <see cref="Session.Code"/> (string) is valid.
    /// </summary>
    public abstract Task<ActionResult<Session>> ValidateSession(string sessionCode, Source source = Source.INTERNAL);
    /// <summary>
    /// Validates that a given <see cref="Session"/> is valid.
    /// </summary>
    public abstract Task<ActionResult<Session>> ValidateSession(Session session, Source source = Source.INTERNAL);

    /// <summary>
    /// Attempt to "login" (..refresh the session) ..of a given <see cref="Account"/> and its hashed password.
    /// </summary>
    /// <param name="userName">Unique Username of an <see cref="Account"/></param>
    /// <param name="hash">SHA-256</param>
    public abstract Task<ActionResult<Session>> Login(string userName, string hash);
    /// <summary>
    /// Attempt to "login" (..refresh the session) ..of a given <see cref="Account"/> and its hashed password.
    /// </summary>
    /// <param name="hash">SHA-256</param>
    public abstract Task<ActionResult<Session>> Login(Account account, string hash);
}