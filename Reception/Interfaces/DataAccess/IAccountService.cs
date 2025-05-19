using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces.DataAccess;

public interface IAccountService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Account&gt;"/>) set of
    /// <see cref="Account"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public abstract DbSet<Account> GetAccounts();

    /// <summary>
    /// Get the <see cref="Account"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<Account>> GetAccount(int id);

    /// <summary>
    /// Get the <see cref="Account"/> with unique '<paramref ref="username"/>'
    /// </summary>
    public abstract Task<ActionResult<Account>> GetAccountByUsername(string username);

    /// <summary>
    /// Get all <see cref="Account"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Account>>> GetAccounts(int? limit, int? offset, DateTime? lastLoginAfter, string? fullName);

    /// <summary>
    /// Update an <see cref="Account"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Account>> UpdateAccount(MutateAccount mut);

    /// <summary>
    /// Update the Avatar of an <see cref="Account"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Account>> UpdateAccountAvatar(Account user, int photoId);

    /// <summary>
    /// Add a new <see cref="Account"/> to the database.
    /// </summary>
    public abstract Task<ActionResult<Account>> CreateAccount(MutateAccount mut);

    /// <summary>
    /// Delete / Remove an <see cref="Account"/> from the database.
    /// </summary>
    public abstract Task<int> DeleteAccount(MutateAccount mut);
}
