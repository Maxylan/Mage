using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;

namespace Reception.Interfaces.DataAccess;

public interface IClientService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Client&gt;"/>) set of
    /// <see cref="Client"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public abstract DbSet<Client> Clients();

    /// <summary>
    /// Get the <see cref="Client"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public abstract Task<ActionResult<Client>> GetClient(int id);

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Client>>> GetClients(
        string? address,
        string? userAgent,
        int? userId,
        string? username,
        int? limit,
        int? offset
    );

    /// <summary>
    /// Update a <see cref="Client"/> in the database to record a visit.
    /// </summary>
    public abstract Task<ActionResult<Client>> RecordVisit(bool successful);

    /// <summary>
    /// Delete / Remove an <see cref="Client"/> from the database.
    /// </summary>
    public abstract Task<int> DeleteClient(int clientId);
}
