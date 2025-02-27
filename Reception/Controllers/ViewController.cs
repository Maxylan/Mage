using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Models;
using Reception.Models.Entities;
using Reception.Interfaces;
using Reception.Utilities;
using Reception.Constants;

namespace Reception.Controllers;

[ApiController]
[Route("links/view")]
[Produces("application/octet-stream")]
public class ViewController(ILinkService handler, IPhotoService photos) : ControllerBase
{
    /// <summary>
    /// View the <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    [HttpGet("source/{code:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Link>> ViewSource(Guid code)
    {
        var getLink = await handler.GetLinkByCode(code);
        var link = getLink.Value;

        if (link is null) {
            return NotFound();
        }

        try {

        }
    }
}
