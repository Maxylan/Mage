using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces;

public interface IBanHandler
{
    /// <summary>
    /// Get the <see cref="BanEntry"/> with Primary Key '<paramref ref="entryId"/>'
    /// </summary>
    public abstract Task<ActionResult<BanEntryDTO>> GetBanEntry(int entryId);

    /// <summary>
    /// Get all <see cref="BanEntry"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<BanEntryDTO>>> GetBannedClients(
        string? address,
        string? userAgent,
        int? userId,
        string? username,
        int? limit = 99,
        int? offset = 0
    );

    /// <summary>
    /// Update a <see cref="BanEntry"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<(BanEntryDTO, bool)>> UpdateBanEntry(MutateBanEntry mut);

    /// <summary>
    /// Create a <see cref="BanEntry"/> in the database.
    /// Equivalent to banning a single client (<see cref="Client"/>).
    /// </summary>
    public abstract Task<ActionResult<BanEntryDTO>> BanClient(MutateBanEntry mut);

    /// <summary>
    /// Delete / Remove a <see cref="BanEntry"/> from the database.
    /// Equivalent to unbanning a single client (<see cref="Client"/>).
    /// </summary>
    public abstract Task<ActionResult> UnbanClient(int entryId);
}
