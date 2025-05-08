using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Models;
using Reception.Database.Models;
using Reception.Interfaces;
using Reception.Utilities;
using Reception.Constants;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("albums")]
[Produces("application/json")]
public class AlbumsController(IAlbumService handler, ITagService tagService) : ControllerBase
{
    /// <summary>
    /// Get a single <see cref="Album"/> by its <paramref name="album_id"/> (PK, uint).
    /// </summary>
    [HttpGet("{album_id:int}")]
    [Tags(ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Album>> GetAlbum(int album_id) =>
        await handler.GetAlbum(album_id);

    /// <summary>
    /// Get / Query for many <see cref="Album"/> instances that match provided search criterias passed as URL/Query Parameters.
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
    public async Task<ActionResult<IEnumerable<Album>>> GetAlbums(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] string[]? tags = null,
        [FromQuery] int? createdBy = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.GetAlbums(opts =>
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
    /// Get the <see cref="Album"/> with PK <paramref ref="album_id"/> (int), along with a collection of all associated Photos.
    /// </summary>
    /// <returns>
    /// <seealso cref="AlbumPhotoCollection"/>
    /// </returns>
    [HttpGet("{album_id:int}/photos")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlbumPhotoCollection>> GetAlbumPhotoCollection(int album_id) =>
        await handler.GetAlbumPhotoCollection(album_id);

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
    public async Task<ActionResult<Album>> CreateAlbum(MutateAlbum mut) =>
        await handler.CreateAlbum(mut);

    /// <summary>
    /// Update the properties of the <see cref="Album"/> with '<paramref ref="album_id"/>' (string), *not* its members (i.e Photos or Albums).
    /// </summary>
    [HttpPut]
    [Tags(ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Album>> UpdateAlbum(MutateAlbum mut) =>
        await handler.UpdateAlbum(mut);

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photo_ids"/> (int[]).
    /// </summary>
    [HttpPut("{album_id:int}/photos")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlbumPhotoCollection>> MutatePhotos(int album_id, [FromBody] int[] photo_ids) =>
        await handler.MutateAlbumPhotos(album_id, photo_ids);

    /// <summary>
    /// Remove a single <see cref="PhotoEntity"/> (<paramref name="photo_id"/>, int) ..from a single <see cref="Album"/> identified by PK '<paramref ref="album_id"/>' (int)
    /// </summary>
    [HttpPatch("{album_id:int}/remove/photo/{photo_id:int}")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemovePhoto(int album_id, int photo_id) =>
        await handler.RemovePhoto(album_id, photo_id);

    /// <summary>
    /// Edit what tags are associated with this <see cref="Album"/>.
    /// </summary>
    [HttpPut("{album_id:int}/tags")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.TAGS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Tag>>> MutateTags(int album_id, [FromBody] string[] tags) =>
        await tagService.MutateAlbumTags(album_id, tags);

    /// <summary>
    /// Remove a single <see cref="Tag"/> (<paramref name="tag"/>, string) ..from a single <see cref="Album"/> identified by PK '<paramref ref="album_id"/>' (int)
    /// </summary>
    [HttpPatch("{album_id:int}/remove/tag/{tag}")]
    [Tags(ControllerTags.ALBUMS, ControllerTags.TAGS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveTag(int album_id, string tag) =>
        await handler.RemoveTag(album_id, tag);

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
