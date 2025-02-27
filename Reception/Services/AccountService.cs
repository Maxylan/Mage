
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Reception.Models;
using Reception.Interfaces;
using System.Net;

namespace Reception.Services;

public class AccountService(
    ILoggingService loggingService,
    MageDbContext db
) : IAccountService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Account&gt;"/>) set of
    /// <see cref="Account"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public DbSet<Account> GetAccounts() =>
        db.Accounts;

    /// <summary>
    /// Get the <see cref="Account"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
        Account? user = await db.Accounts
            .Include(account => account.Sessions)
            .FirstOrDefaultAsync(account => account.Id == id);

        if (user is null)
        {
            string message = $"Failed to find an {nameof(Account)} with ID #{id}.";
            await loggingService
                .Action(nameof(GetAccount))
                .ExternalDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return user;
    }

    /// <summary>
    /// Get all <see cref="Account"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts(int? limit, int? offset, DateTime? lastVisit, string? fullName)
    {
        IQueryable<Account> query = db.Accounts.OrderByDescending(account => account.CreatedAt);
        string message;

        if (lastVisit is not null)
        {
            query = query.Where(account => account.LastVisit >= lastVisit);
        }
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            query = query.Where(account => account.FullName == fullName);
        }

        if (offset is not null)
        {
            if (offset < 0)
            {
                message = $"Parameter {nameof(offset)} has to either be `0`, or any positive integer greater-than `0`.";
                await loggingService
                    .Action(nameof(GetAccount))
                    .LogDebug(message)
                    .SaveAsync();

                return new BadRequestObjectResult(message);
            }

            query = query.Skip(offset.Value);
        }

        if (limit is not null)
        {
            if (limit <= 0)
            {
                message = $"Parameter {nameof(limit)} has to be a positive integer greater-than `0`.";
                await loggingService
                    .Action(nameof(GetAccount))
                    .LogDebug(message)
                    .SaveAsync();

                return new BadRequestObjectResult(message);
            }

            query = query.Take(limit.Value);
        }

        var getAccounts = await query.ToArrayAsync();
        return getAccounts;
    }

    /// <summary>
    /// Update an <see cref="Account"/> in the database.
    /// </summary>
    public async Task<ActionResult<Account>> UpdateAccount(MutateAccount mut)
    {
        var account = await db.Accounts.FindAsync(mut.Id);
        if (account is null)
        {
            string message = $"Failed to find {nameof(Account)} with ID #{mut.Id}";
            await loggingService
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(message);
        }

        account.Email = mut.Email;
        account.Username = mut.Username;
        account.FullName = mut.FullName;
        account.Permissions = mut.Permissions;
        account.AvatarId = mut.AvatarId;

        // Not changed during update..
        // account.Password = mut.Password
        // account.CreatedAt = mut.CreatedAt
        // account.LastVisit = mut.LastVisit

        try
        {
            db.Update(account);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)}. ";
            await loggingService
                .Action(nameof(UpdateAccount))
                .InternalError(message + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}'. ";
            await loggingService
                .Action(nameof(UpdateAccount))
                .InternalError(message + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return account;
    }

    // TODO!

    /// <summary>
    /// Update the Avatar of an <see cref="Account"/> in the database.
    /// </summary>
    public async Task<ActionResult<Account>> UpdateAccountAvatar(Account user, int photo_id)
    {
        throw new NotImplementedException();
    }

    // TODO, maybe?

    /// <summary>
    /// Add a new <see cref="Account"/> to the database.
    /// </summary>
    /* public async Task<ActionResult<Account>> CreateAccount(MutateAccount mut)
    {
        throw new NotImplementedException();
    } */

    /// <summary>
    /// Delete / Remove an <see cref="Account"/> from the database.
    /// </summary>
    /* public async Task<int> DeleteAccount(MutateAccount mut)
    {
        throw new NotImplementedException();
    } */
}
