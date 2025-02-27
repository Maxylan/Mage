using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Reception.Interfaces;
using Reception.Models;
using Reception.Models.Entities;

namespace Reception.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    IAuthorizationService _handler;
    ISessionService _sessions;

    public AuthController(IAuthorizationService authorization, ISessionService sessions)
    {
        _handler = authorization;
        _sessions = sessions;
    }

    /// <summary>
    /// Validates that a session (..inferred from `<see cref="HttpContext"/>`) ..exists and is valid.
    /// </summary>
    [HttpHead("session/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    public async Task<IStatusCodeActionResult> ValidateSession()
    {
        var sessionValidation = await _handler.ValidateSession();
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
    public async Task<ActionResult<Session>> GetSessionDetails([FromRoute] int id) => 
        await _sessions.GetSessionById(id);

    /// <summary>
    /// Attempt to grab a full `<see cref="Session"/>` instance, identified by unique <paramref name="session"/> code (string).
    /// </summary>
    [HttpGet("session/code/{session}")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Session>> GetSessionDetailsByCode([FromRoute] string session) => 
        await _sessions.GetSession(session);

    /// <summary>
    /// Attempt to login a user, creating a new `<see cref="Session"/>` instance.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Session>> Login([FromBody] Login body) => 
        await _handler.Login(body.Username, body.Hash);
}
