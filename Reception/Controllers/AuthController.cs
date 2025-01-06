using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Reception.Interfaces;
using Reception.Models;

namespace Reception.Controllers;

[ApiController]
[Route("[controller]")]
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
    /// 
    /// </summary>
    [HttpHead("session")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)] // TODO!
    public async Task<IStatusCodeActionResult> ValidateSession() => 
        await _handler.ValidateSession(HttpContext);

    [HttpGet("session/{session}")]
    public async Task<ActionResult<Session?>> GetSession([FromRoute] string session) => 
        await _sessions.GetSession(session);
}
