using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Reception.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    public AuthController()
    { }

    /// <summary>
    /// 
    /// </summary>
    [HttpHead("session")]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status200OK)] // TODO!
    public IStatusCodeActionResult ValidateSession()
    {
        throw new NotImplementedException();
    }

    [HttpGet("session/{session}")]
    public IStatusCodeActionResult ValidateSession([FromRoute] string session)
    {
        throw new NotImplementedException();
    }
}
