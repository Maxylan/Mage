using Microsoft.EntityFrameworkCore;
using Reception.Interfaces.DataAccess;
using Reception.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Services.DataAccess;

public class ClientService() : IClientService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;Client&gt;"/>) set of
    /// <see cref="Client"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public DbSet<Client> Clients() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Client"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<Client>> GetClient(int id) {
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
    public async Task<ActionResult<Client>> RecordVisit(bool successful) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete / Remove an <see cref="Client"/> from the database.
    /// </summary>
    public async Task<int> DeleteClient(int clientId) {
        throw new NotImplementedException();
    }
}
