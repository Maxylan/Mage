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
[Produces("application/json")]
public class AuthController(
    ReceptionAuthorizationService authorization,
    ISessionService sessions
    ) : ControllerBase
{
    /// <summary>
    /// Validates that a session (..inferred from `<see cref="HttpContext"/>`) ..exists and is valid.
    /// </summary>
    [Authorize]
    [HttpHead("session/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public IStatusCodeActionResult ValidateSession()
    {
        /* var sessionValidation = await authorization.ValidateSession();
        var session = sessionValidation.Value;

        if (session is null || string.IsNullOrWhiteSpace(session.Code))
        {
            if (sessionValidation.Result is IStatusCodeActionResult statusCodeResult && 
                statusCodeResult.StatusCode != StatusCodes.Status200OK
            ) {
                return statusCodeResult;
            }

            return StatusCode(StatusCodes.Status401Unauthorized);
        } */

        return StatusCode(StatusCodes.Status200OK);
    }

    /// <summary>
    /// Attempt to grab a full `<see cref="Session"/>` instance, identified by PK (uint) <paramref name="id"/>.
    /// </summary>
    [Authorize]
    [HttpGet("session/{id:int}")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Session>> GetSessionDetails([FromRoute] int id) => 
        await sessions.GetSessionById(id);

    /// <summary>
    /// Attempt to grab a full `<see cref="Session"/>` instance, identified by unique <paramref name="session"/> code (string).
    /// </summary>
    [Authorize]
    [HttpGet("session/code/{session}")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status408RequestTimeout)]
    public async Task<ActionResult<Session>> Login([FromBody] Login body) => 
        await authorization.Login(body.Username, body.Hash);
}
