using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Middleware.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;
using Reception.Database;

namespace Reception.Services.DataAccess;

public class ClientService(
    IHttpContextAccessor contextAccessor,
    ILoggingService<ClientService> logging,
    MageDb db
) : IClientService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Client&gt;"/>) set of
    /// <see cref="Client"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public DbSet<Client> Clients() =>
        db.Clients;

    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;BanEntry&gt;"/>) set of
    /// <see cref="BanEntry"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public DbSet<BanEntry> BanEntries() =>
        db.BannedClients;

    /// <summary>
    /// Get the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>'
    /// </summary>
    public async Task<ActionResult<Client>> GetClient(int clientId)
    {
        if (clientId <= 0)
        {
            string message = $"Parameter {nameof(clientId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(GetClient))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
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
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(GetClient))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredViewPrivilege = (byte)
            (Privilege.VIEW | Privilege.VIEW_ALL);

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(GetClient))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        Client? client = await db.Clients.FindAsync(clientId);

        if (client is null)
        {
            string message = $"Failed to find a {nameof(Client)} matching the given ID #{clientId}.";
            logging
                .Action(nameof(GetClient))
                .LogDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        // Load missing navigation entries.
        foreach (var navigation in db.Entry(client).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        foreach (var account in client.Accounts)
        {
            foreach (var navigation in db.Entry(account).Navigations)
            {
                if (!navigation.IsLoaded)
                {
                    await navigation.LoadAsync();
                }
            }
        }

        return client;
    }

    /// <summary>
    /// Get the <see cref="Client"/> with Fingerprint '<paramref ref="address"/>' & '<paramref ref="userAgent"/>'.
    /// </summary>
    public async Task<ActionResult<Client>> GetClientByFingerprint(string address, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            string message = $"Parameter {nameof(address)} cannot be null/omitted!";
            logging
                .Action(nameof(GetClientByFingerprint))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
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
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(GetClientByFingerprint))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredViewPrivilege = (byte)
            (Privilege.VIEW | Privilege.VIEW_ALL);

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(GetClientByFingerprint))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        Client? client = await db.Clients
            .Include(client => client.Accounts)
            .Include(client => client.BanEntries)
            .FirstOrDefaultAsync(client => client.Address == address && client.UserAgent == userAgent);

        if (client is null)
        {
            string message = $"Failed to find a {nameof(Client)} matching the given address '{address}'";
            if (!string.IsNullOrWhiteSpace(userAgent)) {
                message += " and user agent";
            }

            logging
                .Action(nameof(GetClientByFingerprint))
                .LogDebug(message + "!", opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return client;
    }

    /// <summary>
    /// Check if the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>' (int) is banned.
    /// </summary>
    public async Task<ActionResult<BanEntry?>> IsBanned(int clientId)
    {
        if (clientId <= 0)
        {
            string message = $"Parameter {nameof(clientId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(IsBanned))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
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
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(IsBanned))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredViewPrivilege = (byte)
            (Privilege.VIEW | Privilege.VIEW_ALL);

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(IsBanned))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        Client? client = await db.Clients
            .Include(client => client.BanEntries)
            .FirstOrDefaultAsync(client => client.Id == clientId);

        if (client is null)
        {
            string message = $"Failed to find a {nameof(Client)} matching the given ID #{clientId}!";
            logging
                .Action(nameof(IsBanned))
                .LogDebug(message + "!", opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return client.BanEntries
            .FirstOrDefault(entry => entry.ExpiresAt >= DateTime.Now);
    }

    /// <summary>
    /// Check if the <see cref="Client"/> '<paramref ref="client"/>' is banned.
    /// </summary>
    public async Task<ActionResult<BanEntry?>> IsBanned(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);

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
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(IsBanned))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredViewPrivilege = (byte)
            (Privilege.VIEW | Privilege.VIEW_ALL);

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(IsBanned))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        // Load missing navigation entries.
        foreach (var navigation in db.Entry(client).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        return client.BanEntries
            .FirstOrDefault(entry => entry.ExpiresAt >= DateTime.Now);
    }

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Client>>> GetClients(Action<FilterClients> opts)
    {
        FilterClients filtering = new();
        opts(filtering);

        return GetClients(filtering);
    }

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Client>>> GetClients(FilterClients filter)
    {
        Account? user;
        try
        {
            user = MemoAuth.GetAccount(contextAccessor);

            if (user is null) {
                logging
                    .Action(nameof(GetClients))
                    .InternalSuspicious("Prevented attempted unauthorized access.")
                    .LogAndEnqueue();

                return Array.Empty<Client>();
            }
        }
        catch (Exception ex)
        {
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(GetClients))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return Array.Empty<Client>();
        }

        byte requiredViewPrivilege = (byte)
            (Privilege.VIEW | Privilege.VIEW_ALL);

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(GetClients))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        IQueryable<Client> clientsQuery = db.Clients
            .Include(client => client.Accounts)
            .Include(client => client.BanEntries);

        if (filter.offset is not null)
        {
            if (filter.offset < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(filter.offset),
                    filter.offset,
                    $"Pagination Parameter {nameof(filter.offset)} has to be a positive integer!"
                );
            }

            clientsQuery = clientsQuery
                .Skip(filter.offset.Value);
        }
        if (filter.limit is not null)
        {
            if (filter.limit <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(filter.limit),
                    filter.limit,
                    $"Pagination Parameter {nameof(filter.limit)} has to be a non-zero positive integer!"
                );
            }

            clientsQuery = clientsQuery
                .Take(filter.limit.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.address))
        {
            clientsQuery = clientsQuery
                .Where(client => client.Address == filter.address);
        }

        if (!string.IsNullOrWhiteSpace(filter.userAgent))
        {
            clientsQuery = clientsQuery
                .Where(client => client.UserAgent == filter.userAgent);
        }

        if (!string.IsNullOrWhiteSpace(filter.username))
        {
            clientsQuery = clientsQuery
                .Where(client => client.Accounts.Any(account => account.Username == filter.username));
        }

        var clients = await clientsQuery
            .ToArrayAsync();

        return clients
            .OrderBy(client => client.CreatedAt)
            .OrderByDescending(client => client.Accounts.Count)
            .ToArray();
    }

    /// <summary>
    /// Update a <see cref="Client"/> in the database to record a visit.
    /// </summary>
    /// <remarks>
    /// <paramref name="successfull"/> dictates which parameter should be incremented
    /// (<see cref="Client.Logins"/> or <see cref="Client.FailedLogins"/>)
    /// </remarks>
    public ActionResult RecordVisit(ref Client client, bool successfull)
    {
        ArgumentNullException.ThrowIfNull(client);

        switch(successfull) {
            case true:
                client.Logins++;
                break;
            case false:
                client.FailedLogins++;
                break;
        }

        try
        {
            switch(db.Entry(client).State) {
                case EntityState.Added:
                    break;
                case EntityState.Deleted:
                    return new StatusCodeResult(StatusCodes.Status304NotModified);
                default:
                    db.Update(client);
                    break;
            }

            db.SaveChanges();
        }
        catch (Exception ex)
        {
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(RecordVisit)}!";
            logging
                .Action(nameof(RecordVisit))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : message
            ) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return new OkResult();
    }

    /*
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
    */

    /// <summary>
    /// Delete / Remove an <see cref="Client"/> from the database.
    /// </summary>
    public async Task<ActionResult> DeleteClient(int clientId)
    {
        ArgumentNullException.ThrowIfNull(clientId, nameof(clientId));

        if (clientId <= 0)
        {
            string message = $"Parameter '{nameof(clientId)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(DeleteClient))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
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
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(DeleteClient))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.DELETE | Privilege.ADMIN);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(DeleteClient))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        Client? existingClient = await db.Clients.FindAsync(clientId);

        if (existingClient is null)
        {
            string message = $"{nameof(Client)} with ID #{clientId} could not be found!";
            logging
                .Action(nameof(DeleteClient))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        try
        {
            db.Remove(existingClient);

            logging
                .Action(nameof(DeleteClient))
                .ExternalWarning(
                    $"The {nameof(Client)} '{existingClient.Address}' was just deleted.",
                    opts => {
                        opts.SetUser(user);
                    }
                )
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(Client)} '{existingClient.Address}'. ";
            logging
                .Action(nameof(DeleteClient))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(Client)} '{existingClient.Address}'. ";
            logging
                .Action(nameof(DeleteClient))
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

        return new NoContentResult();
    }
}
