using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database;
using Reception.Models;
using System.Net;

namespace Reception.Services.DataAccess;

public class AccountService(
    ILoggingService<AccountService> logging,
    MageDb db
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
            logging
                .Action(nameof(GetAccount))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return user;
    }

    /// <summary>
    /// Get the <see cref="Account"/> with unique '<paramref ref="username"/>'
    /// </summary>
    public async Task<ActionResult<Account>> GetAccountByUsername(string username)
    {
        Account? user = await db.Accounts
            .Include(account => account.Sessions)
            .FirstOrDefaultAsync(account => account.Username == username);

        if (user is null)
        {
            string message = $"Failed to find an {nameof(Account)} with Username '{username}'.";
            logging
                .Action(nameof(GetAccountByUsername))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return user;
    }

    /// <summary>
    /// Get all <see cref="Account"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts(int? limit, int? offset, DateTime? lastLoginAfter, string? fullName)
    {
        IQueryable<Account> query = db.Accounts.OrderByDescending(account => account.CreatedAt);
        string message;

        if (lastLoginAfter is not null)
        {
            query = query.Where(account => account.LastLogin >= lastLoginAfter);
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
                logging
                    .Action(nameof(GetAccount))
                    .LogDebug(message)
                    .LogAndEnqueue();

                return new BadRequestObjectResult(message);
            }

            query = query.Skip(offset.Value);
        }

        if (limit is not null)
        {
            if (limit <= 0)
            {
                message = $"Parameter {nameof(limit)} has to be a positive integer greater-than `0`.";
                logging
                    .Action(nameof(GetAccount))
                    .LogDebug(message)
                    .LogAndEnqueue();

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
            logging
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        if (string.IsNullOrWhiteSpace(mut.Email)) {
            string message = $"{nameof(Account.Email)} cannot be null/empty!";
            logging
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }
        else if (mut.Email.Length > 255) {
            string message = $"{nameof(Account.Email)} length cannot exceed 255 characters!";
            logging
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (string.IsNullOrWhiteSpace(mut.Username)) {
            string message = $"{nameof(Account.Username)} cannot be null/empty!";
            logging
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }
        else if (mut.Username.Length > 127) {
            string message = $"{nameof(Account.Username)} length cannot exceed 127 characters!";
            logging
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (string.IsNullOrWhiteSpace(mut.FullName)) {
            string message = $"{nameof(Account.FullName)} cannot be null/empty!";
            logging
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }
        else if (mut.FullName.Length > 255) {
            string message = $"{nameof(Account.FullName)} length cannot exceed 255 characters!";
            logging
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (mut.AvatarId <= 0)
        {
            string message = $"'{nameof(MutateAccount.AvatarId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(UpdateAccountAvatar))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (account.AvatarId != mut.AvatarId) {
            var photoExists = await db.Photos.AnyAsync(photo => photo.Id == mut.AvatarId);
            if (!photoExists)
            {
                string message = $"Failed to find {nameof(Photo)} with ID #{mut.AvatarId}";
                logging
                    .Action(nameof(UpdateAccount))
                    .LogDebug(message)
                    .LogAndEnqueue();

                return new NotFoundObjectResult(message);
            }
        }

        account.Email = mut.Email;
        account.Username = mut.Username;
        account.FullName = mut.FullName;
        account.Privilege = mut.Privilege;
        account.AvatarId = mut.AvatarId;

        // Not changed during update..
        // account.Password = mut.Password
        // account.CreatedAt = mut.CreatedAt
        // account.LastLogin = mut.LastLogin

        try
        {
            db.Update(account);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)}. ";
            logging
                .Action(nameof(UpdateAccount))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .LogAndEnqueue();

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
            logging
                .Action(nameof(UpdateAccount))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return account;
    }

    /// <summary>
    /// Update the Avatar of an <see cref="Account"/> in the database.
    /// </summary>
    public async Task<ActionResult<Account>> UpdateAccountAvatar(Account user, int photoId)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(UpdateAccountAvatar))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (user.AvatarId == photoId) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var photoExists = await db.Photos.AnyAsync(photo => photo.Id == photoId);
        if (!photoExists)
        {
            string message = $"Failed to find {nameof(Photo)} with ID #{photoId}";
            logging
                .Action(nameof(UpdateAccount))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        try
        {
            user.AvatarId = photoId;
            db.Update(user);

            if (Program.IsDevelopment)
            {
                logging
                    .Action(nameof(UpdateAccountAvatar))
                    .InternalInformation($"{nameof(Account)} '{user.Username}' (#{user.Id}) just had its Avatar updated.")
                    .LogAndEnqueue();
            }

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update the Avatar of User '{user.Username}'. ";
            logging
                .Action(nameof(UpdateAccountAvatar))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update the Avatar of User '{user.Username}'. ";
            logging
                .Action(nameof(UpdateAccountAvatar))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return user;
    }

    /// <summary>
    /// Add a new <see cref="Account"/> to the database.
    /// </summary>
    public async Task<ActionResult<Account>> CreateAccount(MutateAccount mut)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete / Remove an <see cref="Account"/> from the database.
    /// </summary>
    public async Task<int> DeleteAccount(MutateAccount mut)
    {
        throw new NotImplementedException();
    }
}
