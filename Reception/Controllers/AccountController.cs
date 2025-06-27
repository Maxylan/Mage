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
[Route("accounts")]
[Produces("application/json")]
public class AccountsController(IAccountHandler handler) : ControllerBase
{
    /// <summary>
    /// Get a single <see cref="AccountDTO"/> (user) by its <paramref name="account_id"/> (PK, uint).
    /// </summary>
    [HttpGet("{account_id:int}")]
    [Tags(ControllerTags.USERS, ControllerTags.ACCOUNTS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDTO>> Get(int account_id) =>
        await handler.GetAccount(account_id);

    /// <summary>
    /// Get all <see cref="AccountDTO"/> (user) -instances, optionally filtered and/or paginated by a few query parameters.
    /// </summary>
    [HttpGet]
    [Tags(ControllerTags.USERS, ControllerTags.ACCOUNTS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AccountDTO>>> GetAll(
        [FromQuery] int? limit,
        [FromQuery] int? offset,
        [FromQuery] DateTime? lastVisit,
        [FromQuery] string? fullName
    ) => await handler.GetAccounts(limit, offset, lastVisit, fullName);

    /// <summary>
    /// Update a single <see cref="AccountDTO"/> (user) in the database.
    /// </summary>
    [HttpPut("{account_id:int}")]
    [Tags(ControllerTags.USERS, ControllerTags.ACCOUNTS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDTO>> Update(int account_id, MutateAccount mut)
    {
        if (mut.Id == default)
        {
            if (account_id == default)
            {
                return BadRequest($"Both parameters '{nameof(account_id)}' and '{nameof(mut.Id)}' are invalid!");
            }

            mut.Id = account_id;
        }

        return await handler.UpdateAccount(mut);
    }

    /// <summary>
    /// Update the avatar of a single <see cref="AccountDTO"/> (user).
    /// </summary>
    [HttpPatch("{account_id:int}/avatar/{photo_id:int}")]
    [Tags(ControllerTags.USERS, ControllerTags.ACCOUNTS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDTO>> UpdateAvatar(int account_id, int photo_id)
    {
        if (account_id == default)
        {
            return BadRequest($"Parameter '{nameof(account_id)}' is invalid!");
        }
        if (photo_id == default)
        {
            return BadRequest($"Parameter '{nameof(photo_id)}' is invalid!");
        }

        var getAccount = await handler.GetAccount(account_id);
        var user = getAccount.Value;
        if (user is null)
        {
            return NotFound();
        }

        return await handler.UpdateAvatar(user, photo_id);
    }
}
