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

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("links")]
[Produces("application/json")]
public class LinksController(
    IPublicLinkHandler handler
) : ControllerBase
{
    /// <summary>
    /// Get the <see cref="Link"/> with Primary Key '<paramref ref="link_id"/>'
    /// </summary>
    [HttpGet("{link_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicLinkDTO>> GetLinkById(int link_id) =>
        await handler.GetLink(link_id);

    /// <summary>
    /// Get the <see cref="Link"/> with Unique '<paramref ref="code"/>' (string)
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicLinkDTO>> GetLinkByCode(string code) =>
        await handler.GetLinkByCode(code);

    /// <summary>
    /// Get all <see cref="Link"/> entries.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PublicLinkDTO>>> GetLinks([Required] int limit = 99, [Required] int offset = 0) =>
        await handler.GetLinks(limit, offset);

    /// <summary>
    /// Get all <strong>*active*</string> <see cref="Link"/> entries.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PublicLinkDTO>>> GetActiveLinks([Required] int limit = 99, [Required] int offset = 0) =>
        await handler.GetLinks(limit, offset);

    /// <summary>
    /// Create a <see cref="Link"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photo_id"/>'.
    /// </summary>
    [HttpPost("{photo_id:int}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicLinkDTO>> CreateLink(int photo_id, [FromBody] MutateLink mut) =>
        await handler.CreateLink(photo_id, mut);

    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    [HttpPut("{link_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicLinkDTO>> UpdateLink(int link_id, [FromBody] MutateLink mut) =>
        await handler.UpdateLink(link_id, mut);

    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/> by 
    /// GUID '<paramref name="code"/>'.
    /// </summary>
    [HttpPut("code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicLinkDTO>> UpdateLinkByCode(string code, [FromBody] MutateLink mut) =>
        await handler.UpdateLinkByCode(code, mut);

    /// <summary>
    /// Delete the <see cref="Link"/> with Primary Key '<paramref ref="link_id"/>'
    /// </summary>
    [HttpDelete("{link_id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicLinkDTO>> DeleteLinkById(int link_id) =>
        await handler.DeleteLink(link_id);

    /// <summary>
    /// Delete the <see cref="Link"/> with GUID '<paramref ref="code"/>'
    /// </summary>
    [HttpDelete("code/{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicLinkDTO>> DeleteLinkByCode(string code) =>
        await handler.DeleteLinkByCode(code);
}
