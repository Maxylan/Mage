using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces.DataAccess;

public interface IClientService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Client&gt;"/>) set of
    /// <see cref="Client"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public abstract DbSet<Client> Clients();

    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;BanEntry&gt;"/>) set of
    /// <see cref="BanEntry"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public abstract DbSet<BanEntry> BanEntries();

    /// <summary>
    /// Get the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>'
    /// </summary>
    public abstract Task<ActionResult<Client>> GetClient(int clientId);

    /// <summary>
    /// Get the <see cref="Client"/> with Fingerprint '<paramref ref="address"/>' & '<paramref ref="userAgent"/>'.
    /// </summary>
    public abstract Task<ActionResult<Client>> GetClientByFingerprint(string address, string? userAgent);

    /// <summary>
    /// Check if the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>' (int) is banned.
    /// </summary>
    public abstract Task<ActionResult<BanEntry?>> IsBanned(int clientId);

    /// <summary>
    /// Check if the <see cref="Client"/> '<paramref ref="client"/>' is banned.
    /// </summary>
    public abstract Task<ActionResult<BanEntry?>> IsBanned(Client client);

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
    public abstract Task<ActionResult<IEnumerable<Client>>> GetClients(FilterClients filter);

    /// <summary>
    /// Update a <see cref="Client"/> in the database to record a visit.
    /// </summary>
    /// <remarks>
    /// <paramref name="successfull"/> dictates which parameter should be incremented
    /// (<see cref="Client.Logins"/> or <see cref="Client.FailedLogins"/>)
    /// </remarks>
    public abstract ActionResult RecordVisit(ref Client client, bool successfull);

    /*
    /// <summary>
    /// Create a <see cref="BanEntry"/> for '<paramref ref="client"/>' <see cref="Client"/>, banning it indefinetly
    /// (or until <paramref name="expiry"/>).
    /// </summary>
    public abstract Task<ActionResult<BanEntry>> CreateBanEntry(Client client, DateTime? expiry = null);
    */

    /// <summary>
    /// Delete / Remove an <see cref="Client"/> from the database.
    /// </summary>
    public abstract Task<ActionResult> DeleteClient(int clientId);
}
