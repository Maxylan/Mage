using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces.DataAccess;

public interface IBannedClientsService
{
    /// <summary>
    /// Get the <see cref="IQueryable"/> (<seealso cref="DbSet&lt;BanEntry&gt;"/>) set of
    /// <see cref="BanEntry"/>-entries, you may use it to freely fetch some users.
    /// </summary>
    public abstract DbSet<BanEntry> BanEntries();

    /// <summary>
    /// Get the <see cref="BanEntry"/> with Primary Key '<paramref ref="entryId"/>'
    /// </summary>
    public abstract Task<ActionResult<BanEntry>> GetBanEntry(int entryId);

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
    public abstract Task<ActionResult<IEnumerable<BanEntry>>> GetBannedClients(FilterBanEntries filter);

    /// <summary>
    /// Update a <see cref="BanEntry"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<BanEntry>> UpdateBanEntry(MutateBanEntry mut);

    /// <summary>
    /// Create a <see cref="BanEntry"/> in the database.
    /// Equivalent to banning a single client (<see cref="Client"/>).
    /// </summary>
    public abstract Task<ActionResult<BanEntry>> CreateBanEntry(MutateBanEntry mut);

    /// <summary>
    /// Delete / Remove a <see cref="BanEntry"/> from the database.
    /// Equivalent to unbanning a single client (<see cref="Client"/>).
    /// </summary>
    public abstract Task<ActionResult<int>> DeleteBanEntry(int entryId);

    /// <summary>
    /// Delete / Remove the <see cref="BanEntry"/> <paramref name="banEntry"/> from
    /// the database. Equivalent to unbanning a single client (<see cref="Client"/>).
    /// </summary>
    public abstract Task<ActionResult<int>> DeleteBanEntry(BanEntry banEntry);
}
