using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Authorization;
using Reception.Database;
using Reception.Database.Models;
using Reception.Models;
using Reception.Interfaces;
using Reception.Constants;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("photos")]
[Produces("application/json")]
public class PhotosController(
    IPhotoHandler handler,
    IPhotoStreamingService photoStreaming,
    ITagHandler tagHandler,
    IBlobService blobs
) : ControllerBase
{
    #region Get single photos.
    /// <summary>
    /// Get a single <see cref="Photo"/> (single source) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [HttpGet("{photo_id:int}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoDTO>> GetSourcePhotoById(int photo_id) =>
        await handler.GetPhoto(photo_id);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single source) by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("slug/{slug}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoDTO>> GetSourcePhotoBySlug(string slug) =>
        await handler.GetPhoto(slug);


    /// <summary>
    /// Get a single <see cref="DisplayPhoto"/> by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [HttpGet("{photo_id:int}/display")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayPhoto>> GetPhotoById(int photo_id) =>
        await handler.GetDisplayPhoto(photo_id);

    /// <summary>
    /// Get a single <see cref="DisplayPhoto"/> by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("slug/{slug}/display")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayPhoto>> GetPhotoBySlug(string slug) =>
        await handler.GetDisplayPhoto(slug);
    #endregion

    #region Get single photo blobs.
    /// <summary>
    /// Get a single <see cref="Photo"/> (single source blob) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("{photo_id:int}/blob/source")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetSourceBlobById(int photo_id) =>
        await blobs.GetSourceBlob(photo_id);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single source blob) by its <paramref name="slug"/> (string).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("slug/{slug}/blob/source")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetSourceBlobBySlug(string slug) =>
        await blobs.GetSourceBlobBySlug(slug);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single medium blob) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("{photo_id:int}/blob/medium")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetMediumBlobById(int photo_id) =>
        await blobs.GetMediumBlob(photo_id);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single medium blob) by its <paramref name="slug"/> (string).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("slug/{slug}/blob/medium")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetMediumBlobBySlug(string slug) =>
        await blobs.GetMediumBlobBySlug(slug);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single thumbnail blob) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("{photo_id:int}/blob/thumbnail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetThumbnailBlobById(int photo_id) =>
        await blobs.GetThumbnailBlob(photo_id);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single thumbnail blob) by its <paramref name="slug"/> (string).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("slug/{slug}/blob/thumbnail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetThumbnailBlobBySlug(string slug) =>
        await blobs.GetThumbnailBlobBySlug(slug);
    #endregion

    #region Get multiple photos.
    /// <summary>
    /// Get many <see cref="Photo"/>'s matching a number of given criterias passed by URL/Query Parameters.
    /// </summary>
    /// <param name="uploadedBefore">
    /// Images uploaded <strong>before</strong> the given date, cannot be used with <paramref name="uploadedAfter"/>
    /// </param>
    /// <param name="uploadedAfter">
    /// Images uploaded <strong>after</strong> the given date, cannot be used with <paramref name="uploadedBefore"/>
    /// </param>
    /// <param name="createdBefore">
    /// Images taken/created <strong>before</strong> the given date, cannot be used with <paramref name="createdAfter"/>
    /// </param>
    /// <param name="createdAfter">
    /// Images taken/created <strong>after</strong> the given date, cannot be used with <paramref name="createdBefore"/>
    /// </param>
    [HttpGet]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PhotoDTO>>> FilterPhotos(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? search = null,
        [FromQuery] string? slug = null,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] string[]? tags = null,
        [FromQuery] int? uploadedBy = null,
        [FromQuery] DateTime? uploadedBefore = null,
        [FromQuery] DateTime? uploadedAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.GetPhotos(opts =>
        {
            opts.Limit = limit;
            opts.Offset = offset;
            opts.Dimension = Dimension.SOURCE;
            opts.Slug = slug;
            opts.Title = title;
            opts.Summary = summary;
            opts.UploadedBy = uploadedBy;
            opts.UploadedBefore = uploadedBefore;
            opts.UploadedAfter = uploadedAfter;
            opts.CreatedBefore = createdBefore;
            opts.CreatedAfter = createdAfter;
            opts.Tags = tags;
        });


    /// <summary>
    /// Get multiple <see cref="PhotoCollection"/>'s matching a number of given criterias passed by URL/Query Parameters.
    /// </summary>
    /// <param name="uploadedBefore">
    /// Images uploaded <strong>before</strong> the given date, cannot be used with <paramref name="uploadedAfter"/>
    /// </param>
    /// <param name="uploadedAfter">
    /// Images uploaded <strong>after</strong> the given date, cannot be used with <paramref name="uploadedBefore"/>
    /// </param>
    /// <param name="createdBefore">
    /// Images taken/created <strong>before</strong> the given date, cannot be used with <paramref name="createdAfter"/>
    /// </param>
    /// <param name="createdAfter">
    /// Images taken/created <strong>after</strong> the given date, cannot be used with <paramref name="createdBefore"/>
    /// </param>
    [HttpGet("display")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DisplayPhoto>>> FilterDisplayPhotos(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? search = null,
        [FromQuery] Dimension? dimension = null,
        [FromQuery] string? slug = null,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] int? uploadedBy = null,
        [FromQuery] DateTime? uploadedBefore = null,
        [FromQuery] DateTime? uploadedAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.GetDisplayPhotos(opts =>
        {
            opts.Limit = limit;
            opts.Offset = offset;
            opts.Dimension = dimension;
            opts.Slug = slug;
            opts.Title = title;
            opts.Summary = summary;
            opts.UploadedBy = uploadedBy;
            opts.UploadedBefore = uploadedBefore;
            opts.UploadedAfter = uploadedAfter;
            opts.CreatedBefore = createdBefore;
            opts.CreatedAfter = createdAfter;
        });

    /// <summary>
    /// Get many <see cref="Photo"/>'s matching a number of given criterias passed by URL/Query Parameters.
    /// </summary>
    /// <param name="uploadedBefore">
    /// Images uploaded <strong>before</strong> the given date, cannot be used with <paramref name="uploadedAfter"/>
    /// </param>
    /// <param name="uploadedAfter">
    /// Images uploaded <strong>after</strong> the given date, cannot be used with <paramref name="uploadedBefore"/>
    /// </param>
    /// <param name="createdBefore">
    /// Images taken/created <strong>before</strong> the given date, cannot be used with <paramref name="createdAfter"/>
    /// </param>
    /// <param name="createdAfter">
    /// Images taken/created <strong>after</strong> the given date, cannot be used with <paramref name="createdBefore"/>
    /// </param>
    [HttpGet("search")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PhotoDTO>>> SearchPhotos(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? search = null,
        [FromQuery] string? slug = null,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] string[]? tags = null,
        [FromQuery] int? uploadedBy = null,
        [FromQuery] DateTime? uploadedBefore = null,
        [FromQuery] DateTime? uploadedAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.PhotoSearch(search ?? string.Empty, opts =>
        {
            opts.Limit = limit;
            opts.Offset = offset;
            opts.Dimension = Dimension.SOURCE;
            opts.Slug = slug;
            opts.Title = title;
            opts.Summary = summary;
            opts.UploadedBy = uploadedBy;
            opts.UploadedBefore = uploadedBefore;
            opts.UploadedAfter = uploadedAfter;
            opts.CreatedBefore = createdBefore;
            opts.CreatedAfter = createdAfter;
            opts.Tags = tags;
        });


    /// <summary>
    /// Get multiple <see cref="PhotoCollection"/>'s matching a number of given criterias passed by URL/Query Parameters.
    /// </summary>
    /// <param name="uploadedBefore">
    /// Images uploaded <strong>before</strong> the given date, cannot be used with <paramref name="uploadedAfter"/>
    /// </param>
    /// <param name="uploadedAfter">
    /// Images uploaded <strong>after</strong> the given date, cannot be used with <paramref name="uploadedBefore"/>
    /// </param>
    /// <param name="createdBefore">
    /// Images taken/created <strong>before</strong> the given date, cannot be used with <paramref name="createdAfter"/>
    /// </param>
    /// <param name="createdAfter">
    /// Images taken/created <strong>after</strong> the given date, cannot be used with <paramref name="createdBefore"/>
    /// </param>
    [HttpGet("search/display")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DisplayPhoto>>> SearchDisplayPhotos(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? search = null,
        [FromQuery] Dimension? dimension = null,
        [FromQuery] string? slug = null,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] int? uploadedBy = null,
        [FromQuery] DateTime? uploadedBefore = null,
        [FromQuery] DateTime? uploadedAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.DisplayPhotosSearch(search ?? string.Empty, opts =>
        {
            opts.Limit = limit;
            opts.Offset = offset;
            opts.Dimension = dimension;
            opts.Slug = slug;
            opts.Title = title;
            opts.Summary = summary;
            opts.UploadedBy = uploadedBy;
            opts.UploadedBefore = uploadedBefore;
            opts.UploadedAfter = uploadedAfter;
            opts.CreatedBefore = createdBefore;
            opts.CreatedAfter = createdAfter;
        });
    #endregion

    #region Upload photos.
    /// <summary>
    /// Upload any amount of photos/files by streaming them one-by-one to disk.
    /// </summary>
    [HttpPost("upload")]
    [RequestTimeout(milliseconds: 60000)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [Tags(ControllerTags.PHOTOS_ENTITIES, ControllerTags.PHOTOS_FILES)]
    public async Task<ActionResult<IEnumerable<DisplayPhoto>>> UploadPhotos(/*
        [FromQuery] string? title = null, // Does not support model binding, whatever that is.
        [FromQuery] string? summary = null,
        [FromQuery] string[]? tags = null
    */) =>
        await photoStreaming.UploadPhotos(/* opts => {
            opts.Title = title;
            opts.Summary = summary;
            opts.Tags = tags;
        } */ opts => { });
    #endregion

    #region Edit photos.
    /// <summary>
    /// Upload any amount of photos/files by streaming them one-by-one to disk.
    /// </summary>
    [HttpPut]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoDTO>> UpdatePhoto(MutatePhoto mut) =>
        await handler.UpdatePhoto(mut);
    #endregion

    /// <summary>
    /// Toggles the 'Favorite' status of a <see cref="Reception.Database.Models.Photo"/> for a single user.
    /// </summary>
    [HttpPatch("{photo_id:int}/favorite")]
    [Tags(ControllerTags.PHOTOS_ENTITIES, ControllerTags.USERS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ToggleFavorite(int photo_id) =>
        await handler.ToggleFavorite(photo_id);

    #region Add / Remove tag(s)
    /// <summary>
    /// Edit tags associated with this <see cref="PhotoEntity"/>.
    /// </summary>
    [HttpPut("{photo_id:int}/tags")]
    [Tags(ControllerTags.PHOTOS_ENTITIES, ControllerTags.TAGS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoTagCollection>> MutateTags(
        int photo_id,
        [FromBody] IEnumerable<ITag> tags
    ) =>
        await tagHandler.MutatePhotoTags(photo_id, tags);
    #endregion

    /// <summary>
    /// Add <see cref="Tag"/>(s) (<paramref name="tags"/>) ..to a <see cref="Photo"/> identified by PK '<paramref ref="photo_id"/>' (int)
    /// </summary>
    [HttpPatch("{photo_id:int}/tags/add")]
    [Tags(ControllerTags.PHOTOS_ENTITIES, ControllerTags.TAGS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TagDTO>>> AddTags(
        int photo_id,
        [FromQuery] IEnumerable<ITag> tags
    ) =>
        await handler.AddTags(photo_id, tags);

    /// <summary>
    /// Remove <see cref="Tag"/>(s) (<paramref name="tags"/>) ..from a <see cref="Photo"/> identified by PK '<paramref ref="photo_id"/>' (int)
    /// </summary>
    [HttpPatch("{photo_id:int}/tags/remove")]
    [Tags(ControllerTags.PHOTOS_ENTITIES, ControllerTags.TAGS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TagDTO>>> RemoveTags(
        int photo_id,
        [FromQuery] IEnumerable<ITag> tags
    ) =>
        await handler.RemoveTags(photo_id, tags);

    /// <summary>
    /// Delete the <see cref="PhotoEntity"/> with '<paramref ref="photo_id"/>' (int).
    /// </summary>
    [HttpDelete("{photo_id:int}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePhoto(int photo_id) =>
        await handler.DeletePhoto(photo_id);
}
