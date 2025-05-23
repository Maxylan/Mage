using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Constants;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("clients")]
[Produces("application/json")]
public class ClientsController(
    IClientsHandler clientsHandler,
    IBanHandler banHandler
) : ControllerBase
{
    /// <summary>
    /// Get the <see cref="BanEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    [HttpGet("bans/{entry_id:int}")]
    [Tags(ControllerTags.USERS, ControllerTags.BANS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BanEntryDTO>> GetBanEntry(int entry_id) =>
        await banHandler.GetBanEntry(entry_id);

    /// <summary>
    /// Get all <see cref="BanEntry"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    [HttpGet("bans")]
    [Tags(ControllerTags.USERS, ControllerTags.BANS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BanEntryDTO>>> GetBannedClients(
        string? address,
        string? userAgent,
        int? userId,
        string? username,
        int? limit = 99,
        int? offset = 0
    ) => await banHandler.GetBannedClients(
        address,
        userAgent,
        userId,
        username,
        limit,
        offset
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
