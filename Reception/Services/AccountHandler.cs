using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Services;

public class AccountHandler(
    ILoggingService<AccountHandler> logging,
    IAccountService accountService
) : IAccountHandler
{
    /// <summary>
    /// Get the <see cref="AccountDTO"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<AccountDTO>> GetAccount(int id)
    {
        var getAccount = await accountService.GetAccount(id);

        if (getAccount.Value is null)
        {
            string message = $"Failed to find an {nameof(Account)} with ID #{id}.";
            logging
                .Action(nameof(AccountHandler.GetAccount))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getAccount.Result!;
        }

        return (AccountDTO)getAccount.Value;
    }

    /// <summary>
    /// Get all <see cref="AccountDTO"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<AccountDTO>>> GetAccounts(int? limit, int? offset, DateTime? lastLoginAfter, string? fullName)
    {
        var getAccounts = await accountService.GetAccounts(
            limit,
            offset,
            lastLoginAfter,
            fullName
        );

        if (getAccounts.Value is null)
        {
            string message = $"Failed to get {nameof(Account)}s.";
            logging
                .Action(nameof(AccountHandler.GetAccounts))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getAccounts.Result!;
        }

        return getAccounts.Value
            .Select(account => (AccountDTO)account)
            .ToArray();
    }

    /// <summary>
    /// Update an <see cref="AccountDTO"/> in the database.
    /// </summary>
    public async Task<ActionResult<AccountDTO>> UpdateAccount(MutateAccount mut)
    {
        var updateAccount = await accountService.UpdateAccount(mut);

        if (updateAccount.Value is null)
        {
            string message = $"Failed to update {nameof(Account)} with ID #{mut.Id}.";
            logging
                .Action(nameof(AccountHandler.UpdateAccount))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return updateAccount.Result!;
        }

        return (AccountDTO)updateAccount.Value;
    }

    /// <summary>
    /// Update the Avatar of an <see cref="AccountDTO"/> in the database.
    /// </summary>
    public async Task<ActionResult<AccountDTO>> UpdateAvatar(Account user, int photoId)
    {
        var updateAccount = await accountService.UpdateAccountAvatar(user, photoId);

        if (updateAccount.Value is null)
        {
            string message = $"Failed to update {nameof(Account)} with ID #{user.Id}.";
            logging
                .Action(nameof(AccountHandler.UpdateAccount))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return updateAccount.Result!;
        }

        return (AccountDTO)user;
    }

    // TODO... maybe?
    /// <summary>
    /// Add a new <see cref="Account"/> to the database.
    /// </summary>
    /* public async Task<ActionResult<Account>> CreateAccount(MutateAccount mut)
    {
        throw new NotImplementedException();
    } */
}
