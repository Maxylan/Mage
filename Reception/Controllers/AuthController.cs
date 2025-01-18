using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using ReceptionAuthorizationService = Reception.Interfaces.IAuthorizationService;
using Reception.Interfaces;
using Reception.Models;
using Reception.Models.Entities;

namespace Reception.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(ReceptionAuthorizationService handler, ISessionService sessions) : ControllerBase
{
    /// <summary>
    /// Validates that a session (..inferred from `<see cref="HttpContext"/>`) ..exists and is valid.
    /// </summary>
    [HttpHead("session/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [Authorize(Policy = "AuthenticatedUserPolicy")]
    public async Task<IStatusCodeActionResult> ValidateSession()
    {
        var sessionValidation = await handler.ValidateSession();
        if (sessionValidation is null) {
            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        return (sessionValidation.Result as IStatusCodeActionResult)!;
    }

    /// <summary>
    /// Attempt to grab a full `<see cref="Session"/>` instance, identified by PK (uint) <paramref name="id"/>.
    /// </summary>
    [HttpGet("session/{id:int}")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "AuthenticatedUserPolicy")]
    public async Task<ActionResult<Session>> GetSessionDetails([FromRoute] int id) => 
        await sessions.GetSessionById(id);

    /// <summary>
    /// Attempt to grab a full `<see cref="Session"/>` instance, identified by unique <paramref name="session"/> code (string).
    /// </summary>
    [HttpGet("session/code/{session}")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "AuthenticatedUserPolicy")]
    public async Task<ActionResult<Session>> GetSessionDetailsByCode([FromRoute] string session) => 
        await sessions.GetSession(session);

    /// <summary>
    /// Attempt to login a user, creating a new `<see cref="Session"/>` instance.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Session>> Login([FromBody] Login body) => 
        await handler.Login(body.Username, body.Hash);
}
