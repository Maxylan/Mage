using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Interfaces.DataAccess;

namespace Reception.Services.DataAccess;

public class BannedClientsService() : IBannedClientsService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;BanEntry&gt;"/>) set of
    /// <see cref="BanEntry"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public DbSet<BanEntry> BanEntries() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="BanEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    public async Task<ActionResult<BanEntry>> GetBanEntry(int id) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all <see cref="BanEntry"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<BanEntry>>> GetBannedClients(
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
    /// Update a <see cref="BanEntry"/> in the database.
    /// </summary>
    public async Task<ActionResult<BanEntry>> UpdateBanEntry(bool successful) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete / Remove a <see cref="BanEntry"/> from the database.
    /// </summary>
    public async Task<int> DeleteBanEntry(int BanEntryId) {
        throw new NotImplementedException();
    }
}
