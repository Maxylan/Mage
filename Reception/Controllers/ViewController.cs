using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Models;
using Reception.Database.Models;
using Reception.Interfaces.DataAccess;
using Reception.Utilities;
using Reception.Constants;
using System.Net;
using Reception.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Reception.Controllers;

[ApiController]
[Route("links/view")]
[Produces("application/octet-stream")]
public class ViewController(IViewService handler) : ControllerBase
{
    /// <summary>
    /// View the <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// Disguises a lot of responses outside of Development, since this is deals with publically available URL's and I don't want to encourage pen-testing or scraping.
    /// <para>
    ///     A valid <see cref="Link"/> that's expired will return an HTTP 410 'Gone' response status.
    /// </para>
    /// </remarks>
    [HttpGet("source/{code:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status410Gone)]
    public async Task<ActionResult<Link>> ViewSource(Guid? code) =>
        await handler.ViewSource(code);

    /// <summary>
    /// View the Medium <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// Disguises a lot of responses outside of Development, since this is deals with publically available URL's and I don't want to encourage pen-testing or scraping.
    /// <para>
    ///     A valid <see cref="Link"/> that's expired will return an HTTP 410 'Gone' response status.
    /// </para>
    /// </remarks>
    [HttpGet("medium/{code:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status410Gone)]
    public async Task<ActionResult<Link>> ViewMedium(Guid? code) =>
        await handler.ViewMedium(code);

    /// <summary>
    /// View the Thumbnail <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// Disguises a lot of responses outside of Development, since this is deals with publically available URL's and I don't want to encourage pen-testing or scraping.
    /// <para>
    ///     A valid <see cref="Link"/> that's expired will return an HTTP 410 'Gone' response status.
    /// </para>
    /// </remarks>
    [HttpGet("thumbnail/{code:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status410Gone)]
    public async Task<ActionResult<Link>> ViewThumbnail(Guid? code) =>
        await handler.ViewThumbnail(code);
}
