using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Services.DataAccess;

public class BannedClientsService() : IBannedClientsService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;BanEntry&gt;"/>) set of
    /// <see cref="BanEntry"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public DbSet<BanEntry> BanEntries()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="BanEntry"/> with Primary Key '<paramref ref="entryId"/>'
    /// </summary>
    public async Task<ActionResult<BanEntry>> GetBanEntry(int entryId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all <see cref="BanEntry"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<BanEntry>>> GetBannedClients(Action<FilterBanEntries> opts)
    {
        FilterBanEntries filtering = new();
        opts(filtering);

        return GetBannedClients(filtering);
    }

    /// <summary>
    /// Get all <see cref="BanEntry"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<BanEntry>>> GetBannedClients(FilterBanEntries? filter)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a <see cref="BanEntry"/> in the database.
    /// </summary>
    public async Task<ActionResult<BanEntry>> UpdateBanEntry(MutateBanEntry mut)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a <see cref="BanEntry"/> in the database.
    /// Equivalent to banning a single client (<see cref="Client"/>).
    /// </summary>
    public async Task<ActionResult<BanEntry>> CreateBanEntry(MutateBanEntry mut)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete / Remove a <see cref="BanEntry"/> from the database.
    /// Equivalent to unbanning a single client (<see cref="Client"/>).
    /// </summary>
    public async Task<ActionResult<int>> DeleteBanEntry(int entryId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete / Remove the <see cref="BanEntry"/> <paramref name="banEntry"/> from
    /// the database. Equivalent to unbanning a single client (<see cref="Client"/>).
    /// </summary>
    public async Task<ActionResult<int>> DeleteBanEntry(BanEntry banEntry)
    {
        throw new NotImplementedException();
    }
}
