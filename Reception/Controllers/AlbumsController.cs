using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Models;
using Reception.Database.Models;
using Reception.Interfaces;
using Reception.Constants;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("albums")]
[Produces("application/json")]
public class AlbumsController(IAlbumHandler handler) : ControllerBase
{
    /// <summary>
    /// Get a single <see cref="AlbumDTO"/> by its <paramref name="album_id"/> (PK, uint).
    /// </summary>
    [HttpGet("{album_id:int}")]
    [Tags(ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayAlbum>> GetAlbum(int album_id) =>
        await handler.GetAlbum(album_id);

    /// <summary>
    /// Get / Query for many <see cref="DisplayAlbum"/> instances filtered by the given parameters passed.
    /// </summary>
    /// <param name="createdBefore">
    /// Albums created <strong>before</strong> the given date, cannot be used with <paramref name="createdAfter"/>
    /// </param>
    /// <param name="createdAfter">
    /// Albums created <strong>after</strong> the given date, cannot be used with <paramref name="createdBefore"/>
    /// </param>
    [HttpGet]
    [Tags(ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DisplayAlbum>>> FilterAlbums(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] string[]? tags = null,
        [FromQuery] int? createdBy = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.FilterAlbums(opts =>
        {
            opts.Limit = limit;
            opts.Offset = offset;
            opts.Title = title;
            opts.Summary = summary;
            opts.CreatedBy = createdBy;
            opts.CreatedBefore = createdBefore;
            opts.CreatedAfter = createdAfter;
            opts.Tags = tags;
        });

    /// <summary>
    /// Get / Query for many <see cref="DisplayAlbum"/> instances that match provided search criterias passed as URL/Query Parameters.
    /// </summary>
    /// <param name="createdBefore">
    /// Albums created <strong>before</strong> the given date, cannot be used with <paramref name="createdAfter"/>
    /// </param>
    /// <param name="createdAfter">
    /// Albums created <strong>after</strong> the given date, cannot be used with <paramref name="createdBefore"/>
    /// </param>
    [HttpGet("search")]
    [Tags(ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DisplayAlbum>>> SearchAlbums(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] string[]? tags = null,
        [FromQuery] int? createdBy = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.SearchForAlbums(opts =>
        {
            opts.Limit = limit;
            opts.Offset = offset;
            opts.Title = title;
            opts.Summary = summary;
            opts.CreatedBy = createdBy;
            opts.CreatedBefore = createdBefore;
            opts.CreatedAfter = createdAfter;
            opts.Tags = tags;
        });

    /// <summary>
    /// Create a new <see cref="Album"/>.
    /// </summary>
    [HttpPost]
    [Tags(ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DisplayAlbum>> CreateAlbum(MutateAlbum mut) =>
        await handler.CreateAlbum(mut);

    /// <summary>
    /// Update the properties of the <see cref="Album"/> with '<paramref ref="album_id"/>' (int).
    /// </summary>
    [HttpPut("{album_id:int}")]
    [Tags(ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DisplayAlbum>> UpdateAlbum(int album_id, [FromBody] MutateAlbum mut)
    {
        if (mut.Id == default)
        {
            if (album_id == default)
            {
                return BadRequest($"Both parameters '{nameof(album_id)}' and '{nameof(mut.Id)}' are invalid!");
            }

            mut.Id = album_id;
        }

        return await handler.UpdateAlbum(mut);
    }

    /// <summary>
    /// Add <see cref="Tag"/>(s) (<paramref name="tags"/>) ..to a <see cref="Album"/> identified by PK '<paramref ref="album_id"/>' (int)
    /// </summary>
    [HttpPatch("{album_id:int}/favorite")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.USERS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ToggleFavorite(int album_id) =>
        await handler.ToggleFavorite(album_id);

    /// <summary>
    /// Add photos (<paramref name="photo_ids"/>, int[]) to a given <see cref="Album"/> (<paramref name="album_id"/>).
    /// </summary>
    [HttpPatch("{album_id:int}/add/photos")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayAlbum>> MutatePhotos(int album_id, [FromBody] int[] photo_ids) =>
        await handler.AddPhotos(album_id, photo_ids);

    /// <summary>
    /// Remove photos (<paramref name="photo_ids"/>, int[]) from a given <see cref="Album"/> (<paramref name="album_id"/>).
    /// </summary>
    [HttpPatch("{album_id:int}/remove/photos")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayAlbum>> RemovePhotos(int album_id, [FromBody] int[] photo_ids) =>
        await handler.RemovePhotos(album_id, photo_ids);

    /// <summary>
    /// Add tags (<paramref name="tags"/>, <see cref="IEnumerable{ITag}"/>) to a given <see cref="Album"/> (<paramref name="album_id"/>).
    /// </summary>
    [HttpPatch("{album_id:int}/add/tags")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.TAGS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TagDTO>>> MutateTags(int album_id, [FromBody] IEnumerable<ITag> tags) =>
        await handler.AddTags(album_id, tags);

    /// <summary>
    /// Remove tags (<paramref name="tags"/>, <see cref="IEnumerable{ITag}"/>) from a given <see cref="Album"/> (<paramref name="album_id"/>).
    /// </summary>
    [HttpPatch("{album_id:int}/remove/tags")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.TAGS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TagDTO>>> RemoveTags(int album_id, [FromBody] IEnumerable<ITag> tags) =>
        await handler.RemoveTags(album_id, tags);

    /// <summary>
    /// Delete the <see cref="Album"/> with '<paramref ref="album_id"/>' (int).
    /// </summary>
    [HttpDelete("{album_id:int}")]
    [Tags(ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAlbum(int album_id) =>
        await handler.DeleteAlbum(album_id);
}
