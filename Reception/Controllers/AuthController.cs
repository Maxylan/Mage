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
    /// In other words this endpoint tests my Authentication Pipeline.
    /// </summary>
    [Authorize]
    [HttpHead("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IStatusCodeActionResult ValidateSession() =>
        StatusCode(StatusCodes.Status200OK);


    /// <summary>
    /// Returns the `<see cref="Account"/>` tied to the requesting client's session (i.e, in our `<see cref="HttpContext"/>` pipeline).
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Account>> Me()
    {
        var sessionValidation = await authorization.ValidateSession(Source.EXTERNAL);
        var session = sessionValidation.Value;

        if (session is null || string.IsNullOrWhiteSpace(session.Code)) {
            return sessionValidation.Result!;
        }

        if (session.User is Account user) {
            return user;
        }

        return await sessions.GetUserBySession(session);
    }

    /// <summary>
    /// Attempt to grab a full `<see cref="Session"/>` instance, identified by PK (uint) <paramref name="id"/>.
    /// </summary>
    [Authorize]
    [HttpGet("session/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Session>> GetSessionDetails([FromRoute] int id)
    {
        await sessions.CleanupSessions(); // Do a little cleaning first..
        return await sessions.GetSessionById(id);
    }


    /// <summary>
    /// Attempt to grab a full `<see cref="Session"/>` instance, identified by unique <paramref name="session"/> code (string).
    /// </summary>
    [Authorize]
    [HttpGet("session/code/{session}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Session>> GetSessionDetailsByCode([FromRoute] string session)
    {
        await sessions.CleanupSessions(); // Do a little cleaning first..
        return await sessions.GetSession(session);
    }

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
