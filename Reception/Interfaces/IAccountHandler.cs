using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces;

public interface IAccountHandler
{
    /// <summary>
    /// Get the <see cref="AccountDTO"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<AccountDTO>> GetAccount(int id);

    /// <summary>
    /// Get all <see cref="AccountDTO"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<AccountDTO>>> GetAccounts(int? limit, int? offset, DateTime? lastLoginAfter, string? fullName);

    /// <summary>
    /// Update an <see cref="AccountDTO"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<AccountDTO>> UpdateAccount(MutateAccount mut);

    /// <summary>
    /// Update the Avatar of an <see cref="AccountDTO"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<AccountDTO>> UpdateAvatar(Account user, int photoId);

    /// <summary>
    /// Add a new <see cref="AccountDTO"/> to the database.
    /// </summary>
    // public abstract Task<ActionResult<AccountDTO>> CreateAccount(MutateAccount mut);
}
