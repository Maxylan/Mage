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
    IClientHandler clientHandler,
    IBanHandler banHandler
) : ControllerBase
{
    /// <summary>
    /// Get the <see cref="Client"/> with Primary Key '<paramref ref="client_id"/>'
    /// </summary>
    [HttpGet("{client_id:int}")]
    [Tags(ControllerTags.USERS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayClient>> GetClient(int client_id) =>
        await clientHandler.GetClient(client_id);

    /// <summary>
    /// Get the <see cref="Client"/> with Fingerprint '<paramref ref="address"/>' & '<paramref ref="userAgent"/>'.
    /// </summary>
    [HttpGet("{address}")]
    [Tags(ControllerTags.USERS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayClient>> GetClientByFingerprint(string address, [FromQuery] string? userAgent) =>
        await clientHandler.GetClientByFingerprint(address, userAgent);

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    [HttpGet]
    [Tags(ControllerTags.USERS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ClientDTO>>> GetClients([FromQuery] FilterClients opts) =>
        await clientHandler.GetClients(opts);

    /// <summary>
    /// Get the <see cref="BanEntry"/> with Primary Key '<paramref ref="id"/>'
    /// </summary>
    [HttpGet("ban/{entry_id:int}")]
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
    [HttpGet("ban")]
    [Tags(ControllerTags.USERS, ControllerTags.BANS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BanEntryDTO>>> GetBannedClients([FromQuery] FilterBanEntries filters)
    {
        if (filters.limit is null || filters.limit <= 0)
        {
            filters.limit = 99;
        }
        if (filters.offset is null || filters.offset < 0)
        {
            filters.offset = 0;
        }

        return await banHandler.GetBannedClients(filters);
    }

    /// <summary>
    /// Update a <see cref="BanEntry"/> in the database.
    /// </summary>
    [HttpPut("ban/{entry_id:int}")]
    [Tags(ControllerTags.USERS, ControllerTags.BANS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<(BanEntryDTO, bool)>> UpdateBanEntry(int entry_id, MutateBanEntry mut)
    {
        if (entry_id <= 0)
        {
            return BadRequest();
        }

        if (mut is null)
        {
            mut = new MutateBanEntry() {
                Id = entry_id
            };
        }
        else if (mut.Id <= 0 || mut.Id != entry_id)
        {
            mut.Id = entry_id;
        }

        return await banHandler.UpdateBanEntry(mut);
    }

    /// <summary>
    /// Create a <see cref="BanEntry"/> in the database.
    /// Equivalent to banning a single client (<see cref="Client"/>).
    /// </summary>
    [HttpPost("ban/{client_id:int}")]
    [Tags(ControllerTags.USERS, ControllerTags.BANS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BanEntryDTO>> BanClient(int client_id, [FromBody] MutateBanEntry? mut)
    {
        if (client_id <= 0)
        {
            return BadRequest();
        }

        if (mut is null)
        {
            mut = new MutateBanEntry() {
                ClientId = client_id
            };
        }
        else if (mut.ClientId <= 0 || mut.ClientId != client_id)
        {
            mut.ClientId = client_id;
        }

        return await banHandler.BanClient(mut);
    }

    /// <summary>
    /// Delete / Remove a <see cref="BanEntry"/> from the database.
    /// Equivalent to unbanning a single client (<see cref="Client"/>).
    /// </summary>
    [HttpDelete("ban/{client_id:int}/account/{account_id:int}")]
    [Tags(ControllerTags.USERS, ControllerTags.BANS)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [NonAction]
    public Task<ActionResult> UnbanClient(int client_id, int account_id) =>
        banHandler.UnbanClient(client_id, account_id);
}
