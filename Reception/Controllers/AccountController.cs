using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Models;
using Reception.Models.Entities;
using Reception.Interfaces;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("accounts")]
[Produces("application/json")]
public class AccountsController(IAccountService handler) : ControllerBase
{
    /// <summary>
    /// Get a single <see cref="Account"/> (user) by its <paramref name="id"/> (PK, uint).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> Get(int id) =>
        await handler.GetAccount(id);

    /// <summary>
    /// Get all <see cref="Account"/> (user) -instances, optionally filtered and/or paginated by a few query parameters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<Account>>> GetAll(
        [FromQuery] int? limit,
        [FromQuery] int? offset,
        [FromQuery] DateTime? lastVisit,
        [FromQuery] string? fullName
    ) => await handler.GetAccounts(limit, offset, lastVisit, fullName);

    /// <summary>
    /// Update a single <see cref="Account"/> (user) in the database.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> Update(int id, MutateAccount mut)
    {
        if (mut.Id == default)
        {
            if (id == default)
            {
                return BadRequest($"Both parameters '{nameof(id)}' and '{nameof(mut.Id)}' are invalid!");
            }

            mut.Id = id;
        }

        return await handler.UpdateAccount(mut);
    }

    // TODO! (2025-01-19)

    /// <summary>
    /// Update the avatar of a single <see cref="Account"/> (user).
    /// </summary>
    [NonAction] // [HttpPatch("{id:int}/avatar/{photo_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> UpdateAvatar(int id, int photo_id)
    {
        if (id == default)
        {
            return BadRequest($"Parameter '{nameof(id)}' is invalid!");
        }
        if (photo_id == default)
        {
            return BadRequest($"Parameter '{nameof(photo_id)}' is invalid!");
        }

        var getAccount = await handler.GetAccount(id);
        var user = getAccount.Value;
        if (user is null)
        {
            return NotFound();
        }

        return await handler.UpdateAccountAvatar(user, photo_id);
    }
}