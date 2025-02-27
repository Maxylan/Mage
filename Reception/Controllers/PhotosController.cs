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
[Route("photos")]
[Produces("application/json")]
public class PhotosController(IPhotoService handler) : ControllerBase
{
    /// <summary>
    /// Get a single <see cref="Account"/> (user) by its <paramref name="id"/> (PK, uint).
    /// </summary>
    /* [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> Get(int id) =>
        await handler.GetAccount(id); */

    /// <summary>
    /// Get all <see cref="Account"/> (user) -instances, optionally filtered and/or paginated by a few query parameters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    /* public async Task<ActionResult<IEnumerable<Account>>> GetAll(
        [FromQuery] int? limit,
        [FromQuery] int? offset,
        [FromQuery] DateTime? lastVisit,
        [FromQuery] string? fullName
    ) => await handler.GetAccounts(limit, offset, lastVisit, fullName); */

    /// <summary>
    /// Update a single <see cref="Account"/> (user) in the database.
    /// </summary>
    [HttpPost("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> Update(int id, MutateAccount mut)
    {
        _defaultFormOptions.MultipartBoundaryLengthLimit;

        // return await handler.UpdateAccount(mut);
    }
}