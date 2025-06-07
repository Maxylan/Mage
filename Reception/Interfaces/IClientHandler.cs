using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces;

public interface IClientHandler
{
    /// <summary>
    /// Get the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>'
    /// </summary>
    public abstract Task<ActionResult<DisplayClient>> GetClient(int clientId);

    /// <summary>
    /// Get the <see cref="Client"/> with Fingerprint '<paramref ref="address"/>' & '<paramref ref="userAgent"/>'.
    /// </summary>
    public abstract Task<ActionResult<DisplayClient>> GetClientByFingerprint(string address, string? userAgent);

    /// <summary>
    /// Check if the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>' (int) is banned.
    /// </summary>
    public abstract Task<ActionResult<BanEntryDTO?>> IsBanned(int clientId);

    /// <summary>
    /// Check if the <see cref="Client"/> '<paramref ref="client"/>' is banned.
    /// </summary>
    public abstract Task<ActionResult<BanEntryDTO?>> IsBanned(Client client);

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<ClientDTO>>> GetClients(Action<FilterClients> opts)
    {
        FilterClients filtering = new();
        opts(filtering);

        return GetClients(filtering);
    }

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<ClientDTO>>> GetClients(FilterClients filter);

    /// <summary>
    /// Delete / Remove an <see cref="Client"/> from the database.
    /// </summary>
    public abstract Task<ActionResult> DeleteClient(int clientId);
}
