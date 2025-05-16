using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Middleware.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;

namespace Reception.Services.DataAccess;

public class ClientService(
    IHttpContextAccessor contextAccessor,
    ILoggingService<ClientService> logging
) : IClientService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Client&gt;"/>) set of
    /// <see cref="Client"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public DbSet<Client> Clients()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;BanEntry&gt;"/>) set of
    /// <see cref="BanEntry"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public DbSet<BanEntry> BanEntries()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Client"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<Client>> GetClient(int id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Client"/> with Fingerprint '<paramref ref="address"/>' & '<paramref ref="userAgent"/>'.
    /// </summary>
    public async Task<ActionResult<Client>> GetClientByFingerprint(string address, string? userAgent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>' (int) is banned.
    /// </summary>
    public Task<ActionResult<BanEntry?>> IsBanned(int clientId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if the <see cref="Client"/> '<paramref ref="client"/>' is banned.
    /// </summary>
    public Task<ActionResult<BanEntry?>> IsBanned(Client client)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Client>>> GetClients(
        string? address,
        string? userAgent,
        int? userId,
        string? username,
        int? limit,
        int? offset
    ) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a <see cref="Client"/> in the database to record a visit.
    /// </summary>
    public async Task<ActionResult<Client>> RecordVisit(bool successful)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a <see cref="BanEntry"/> for '<paramref ref="client"/>' <see cref="Client"/>, banning it indefinetly
    /// (or until <paramref name="expiry"/>).
    /// </summary>
    public async Task<ActionResult<BanEntry>> CreateBanEntry(Client client, DateTime? expiry = null)
    {
        if (contextAccessor.HttpContext is null)
        {
            string message = $"BanClient Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(CreateBanEntry))
                .ExternalError(message)
                .LogAndEnqueue();

            return new ObjectResult(
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : message
            ) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        Account? user;
        try
        {
            user = MemoAuth.GetAccount(contextAccessor);

            if (user is null) {
                return new ObjectResult("Prevented attempted unauthorized access.") {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
        catch (Exception ex)
        {
            string message = $"Potential Unauthorized attempt at ${nameof(CreateBanEntry)}. Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(CreateBanEntry))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if ((user.Privilege & Privilege.ADMIN) != Privilege.ADMIN)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({Privilege.ADMIN}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(CreateBanEntry))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete / Remove an <see cref="Client"/> from the database.
    /// </summary>
    public async Task<int> DeleteClient(int clientId)
    {
        throw new NotImplementedException();
    }
}
