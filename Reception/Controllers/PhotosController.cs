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

[Authorize]
[ApiController]
[Route("photos")]
[Produces("application/json")]
public class PhotosController(IPhotoService handler) : ControllerBase
{
    #region Get single photos.
    /// <summary>
    /// Get a single <see cref="Photo"/> (single source) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [HttpGet("source/{photo_id:int}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Photo>> GetSourcePhotoById(int photo_id) =>
        await handler.GetSinglePhoto(photo_id, Dimension.SOURCE);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single source) by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("source/slug/{slug}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Photo>> GetSourcePhotoBySlug(string slug) =>
        await handler.GetSinglePhoto(slug, Dimension.SOURCE);


    /// <summary>
    /// Get a single <see cref="Photo"/> (single medium) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [HttpGet("medium/{photo_id:int}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Photo>> GetMediumPhotoById(int photo_id) =>
        await handler.GetSinglePhoto(photo_id, Dimension.MEDIUM);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single medium) by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("medium/slug/{slug}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Photo>> GetMediumPhotoBySlug(string slug) =>
        await handler.GetSinglePhoto(slug, Dimension.MEDIUM);


    /// <summary>
    /// Get a single <see cref="Photo"/> (single thumbnail) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [HttpGet("thumbnail/{photo_id:int}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Photo>> GetThumbnailPhotoById(int photo_id) =>
        await handler.GetSinglePhoto(photo_id, Dimension.THUMBNAIL);

    /// <summary>
    /// Get a single <see cref="Photo"/> (single thumbnail) by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("thumbnail/slug/{slug}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Photo>> GetThumbnailPhotoBySlug(string slug) =>
        await handler.GetSinglePhoto(slug, Dimension.THUMBNAIL);


    /// <summary>
    /// Get a single <see cref="PhotoCollection"/> by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [HttpGet("{photo_id:int}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoCollection>> GetPhotoById(int photo_id) =>
        await handler.GetPhoto(photo_id);

    /// <summary>
    /// Get a single <see cref="PhotoCollection"/> by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("slug/{slug}")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoCollection>> GetPhotoBySlug(string slug) =>
        await handler.GetPhoto(slug);
    #endregion

    #region Get single photo blobs.
    /// <summary>
    /// Get a single <see cref="Photo"/> (single source blob) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("source/{photo_id:int}/blob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetSourceBlobById(int photo_id)
    {
        var getSourcePhoto = await handler.GetSinglePhoto(photo_id, Dimension.SOURCE);
        var source = getSourcePhoto.Value;
        if (source is null)
        {
            return getSourcePhoto.Result!;
        }

        string path = Path.Combine(source.Path, source.Filename);
        try
        {
            FileStream fileStream = System.IO.File.OpenRead(path);
            IImageFormat? format = MimeVerifyer.DetectImageFormat(source.Filename, fileStream);

            if (format is null)
            {   // TODO! Handle below scenarios!
                // - Bad filename/extension string
                // - Extension/Filename missmatch
                // - MIME not supported
                // - MIME not a valid image type.
                // - MIME not supported by ImageSharp / Missing ImageFormat
                // - MIME Could not be validated (bad magic numbers)
                throw new NotImplementedException($"{nameof(GetSourceBlobById)} {nameof(format)} is null."); // TODO! Handle!!
                // ..could fallback to "application/octet-stream here" instead of throwing?
            }

            fileStream.Position = 0;
            return File(fileStream, format.DefaultMimeType);
        }
        catch (FileNotFoundException notFound)
        {
            return NotFound(
                $"Cought {nameof(FileNotFoundException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {notFound.Message}" : source.Filename
                )
            );
        }
        catch (UnauthorizedAccessException unauthorizedAccess)
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                $"Cought {nameof(UnauthorizedAccessException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {unauthorizedAccess.Message}" : source.Filename
                )
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Cought Internal Server Error " + (
                    Program.IsDevelopment ? ex.Message : ex.GetType().Name
                )
            );
        }
    }

    /// <summary>
    /// Get a single <see cref="Photo"/> (single source blob) by its <paramref name="slug"/> (string).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("source/slug/{slug}/blob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetSourceBlobBySlug(string slug)
    {
        var getSourcePhoto = await handler.GetSinglePhoto(slug, Dimension.SOURCE);
        var source = getSourcePhoto.Value;
        if (source is null)
        {
            return getSourcePhoto.Result!;
        }

        string path = Path.Combine(source.Path, source.Filename);
        try
        {
            FileStream fileStream = System.IO.File.OpenRead(path);
            IImageFormat? format = MimeVerifyer.DetectImageFormat(source.Filename, fileStream);

            if (format is null)
            {   // TODO! Handle below scenarios!
                // - Bad filename/extension string
                // - Extension/Filename missmatch
                // - MIME not supported
                // - MIME not a valid image type.
                // - MIME not supported by ImageSharp / Missing ImageFormat
                // - MIME Could not be validated (bad magic numbers)
                throw new NotImplementedException($"{nameof(GetSourceBlobById)} {nameof(format)} is null."); // TODO! Handle!!
                // ..could fallback to "application/octet-stream here" instead of throwing?
            }

            fileStream.Position = 0;
            return File(fileStream, format.DefaultMimeType);
        }
        catch (FileNotFoundException notFound)
        {
            return NotFound(
                $"Cought {nameof(FileNotFoundException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {notFound.Message}" : source.Filename
                )
            );
        }
        catch (UnauthorizedAccessException unauthorizedAccess)
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                $"Cought {nameof(UnauthorizedAccessException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {unauthorizedAccess.Message}" : source.Filename
                )
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Cought Internal Server Error " + (
                    Program.IsDevelopment ? ex.Message : ex.GetType().Name
                )
            );
        }
    }

    /// <summary>
    /// Get a single <see cref="Photo"/> (single medium blob) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("medium/{photo_id:int}/blob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetMediumBlobById(int photo_id)
    {
        var getMediumPhoto = await handler.GetSinglePhoto(photo_id, Dimension.MEDIUM);
        var medium = getMediumPhoto.Value;
        if (medium is null)
        {
            return getMediumPhoto.Result!;
        }

        string path = Path.Combine(medium.Path, medium.Filename);
        try
        {
            FileStream fileStream = System.IO.File.OpenRead(path);
            IImageFormat? format = MimeVerifyer.DetectImageFormat(medium.Filename, fileStream);

            if (format is null)
            {   // TODO! Handle below scenarios!
                // - Bad filename/extension string
                // - Extension/Filename missmatch
                // - MIME not supported
                // - MIME not a valid image type.
                // - MIME not supported by ImageSharp / Missing ImageFormat
                // - MIME Could not be validated (bad magic numbers)
                throw new NotImplementedException($"{nameof(GetMediumBlobById)} {nameof(format)} is null."); // TODO! Handle!!
                // ..could fallback to "application/octet-stream here" instead of throwing?
            }

            fileStream.Position = 0;
            return File(fileStream, format.DefaultMimeType);
        }
        catch (FileNotFoundException notFound)
        {
            return NotFound(
                $"Cought {nameof(FileNotFoundException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {notFound.Message}" : medium.Filename
                )
            );
        }
        catch (UnauthorizedAccessException unauthorizedAccess)
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                $"Cought {nameof(UnauthorizedAccessException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {unauthorizedAccess.Message}" : medium.Filename
                )
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Cought Internal Server Error " + (
                    Program.IsDevelopment ? ex.Message : ex.GetType().Name
                )
            );
        }
    }

    /// <summary>
    /// Get a single <see cref="Photo"/> (single medium blob) by its <paramref name="slug"/> (string).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("medium/slug/{slug}/blob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetMediumBlobBySlug(string slug)
    {
        var getMediumPhoto = await handler.GetSinglePhoto(slug, Dimension.MEDIUM);
        var medium = getMediumPhoto.Value;
        if (medium is null)
        {
            return getMediumPhoto.Result!;
        }

        string path = Path.Combine(medium.Path, medium.Filename);
        try
        {
            FileStream fileStream = System.IO.File.OpenRead(path);
            IImageFormat? format = MimeVerifyer.DetectImageFormat(medium.Filename, fileStream);

            if (format is null)
            {   // TODO! Handle below scenarios!
                // - Bad filename/extension string
                // - Extension/Filename missmatch
                // - MIME not supported
                // - MIME not a valid image type.
                // - MIME not supported by ImageSharp / Missing ImageFormat
                // - MIME Could not be validated (bad magic numbers)
                throw new NotImplementedException($"{nameof(GetMediumBlobById)} {nameof(format)} is null."); // TODO! Handle!!
                // ..could fallback to "application/octet-stream here" instead of throwing?
            }

            fileStream.Position = 0;
            return File(fileStream, format.DefaultMimeType);
        }
        catch (FileNotFoundException notFound)
        {
            return NotFound(
                $"Cought {nameof(FileNotFoundException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {notFound.Message}" : medium.Filename
                )
            );
        }
        catch (UnauthorizedAccessException unauthorizedAccess)
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                $"Cought {nameof(UnauthorizedAccessException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {unauthorizedAccess.Message}" : medium.Filename
                )
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Cought Internal Server Error " + (
                    Program.IsDevelopment ? ex.Message : ex.GetType().Name
                )
            );
        }
    }

    /// <summary>
    /// Get a single <see cref="Photo"/> (single thumbnail blob) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("thumbnail/{photo_id:int}/blob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetThumbnailBlobById(int photo_id)
    {
        var getThumbnailPhoto = await handler.GetSinglePhoto(photo_id, Dimension.THUMBNAIL);
        var thumbnail = getThumbnailPhoto.Value;
        if (thumbnail is null)
        {
            return getThumbnailPhoto.Result!;
        }

        string path = Path.Combine(thumbnail.Path, thumbnail.Filename);
        try
        {
            FileStream fileStream = System.IO.File.OpenRead(path);
            IImageFormat? format = MimeVerifyer.DetectImageFormat(thumbnail.Filename, fileStream);

            if (format is null)
            {   // TODO! Handle below scenarios!
                // - Bad filename/extension string
                // - Extension/Filename missmatch
                // - MIME not supported
                // - MIME not a valid image type.
                // - MIME not supported by ImageSharp / Missing ImageFormat
                // - MIME Could not be validated (bad magic numbers)
                throw new NotImplementedException($"{nameof(GetThumbnailBlobById)} {nameof(format)} is null."); // TODO! Handle!!
                // ..could fallback to "application/octet-stream here" instead of throwing?
            }

            fileStream.Position = 0;
            return File(fileStream, format.DefaultMimeType);
        }
        catch (FileNotFoundException notFound)
        {
            return NotFound(
                $"Cought {nameof(FileNotFoundException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {notFound.Message}" : thumbnail.Filename
                )
            );
        }
        catch (UnauthorizedAccessException unauthorizedAccess)
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                $"Cought {nameof(UnauthorizedAccessException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {unauthorizedAccess.Message}" : thumbnail.Filename
                )
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Cought Internal Server Error " + (
                    Program.IsDevelopment ? ex.Message : ex.GetType().Name
                )
            );
        }
    }

    /// <summary>
    /// Get a single <see cref="Photo"/> (single thumbnail blob) by its <paramref name="slug"/> (string).
    /// </summary>
    [Tags(ControllerTags.PHOTOS_FILES)]
    [HttpGet("thumbnail/slug/{slug}/blob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status423Locked)]
    public async Task<ActionResult/*FileContentResult*/> GetThumbnailBlobBySlug(string slug)
    {
        var getThumbnailPhoto = await handler.GetSinglePhoto(slug, Dimension.THUMBNAIL);
        var thumbnail = getThumbnailPhoto.Value;
        if (thumbnail is null)
        {
            return getThumbnailPhoto.Result!;
        }

        string path = Path.Combine(thumbnail.Path, thumbnail.Filename);
        try
        {
            FileStream fileStream = System.IO.File.OpenRead(path);
            IImageFormat? format = MimeVerifyer.DetectImageFormat(thumbnail.Filename, fileStream);

            if (format is null)
            {   // TODO! Handle below scenarios!
                // - Bad filename/extension string
                // - Extension/Filename missmatch
                // - MIME not supported
                // - MIME not a valid image type.
                // - MIME not supported by ImageSharp / Missing ImageFormat
                // - MIME Could not be validated (bad magic numbers)
                throw new NotImplementedException($"{nameof(GetThumbnailBlobById)} {nameof(format)} is null."); // TODO! Handle!!
                // ..could fallback to "application/octet-stream here" instead of throwing?
            }

            fileStream.Position = 0;
            return File(fileStream, format.DefaultMimeType);
        }
        catch (FileNotFoundException notFound)
        {
            return NotFound(
                $"Cought {nameof(FileNotFoundException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {notFound.Message}" : thumbnail.Filename
                )
            );
        }
        catch (UnauthorizedAccessException unauthorizedAccess)
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                $"Cought {nameof(UnauthorizedAccessException)} attempting to open file " + (
                    Program.IsDevelopment ? $"'{path}'. {unauthorizedAccess.Message}" : thumbnail.Filename
                )
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Cought Internal Server Error " + (
                    Program.IsDevelopment ? ex.Message : ex.GetType().Name
                )
            );
        }
    }
    #endregion

    #region Get multiple photos.
    /// <summary>
    /// Get many <see cref="Photo"/>'s singles (source-images) matching a number of given criterias passed by URL/Query Parameters.
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
    [HttpGet("source")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<Photo>>> GetSourceSingles(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? slug = null,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] int? uploadedBy = null,
        [FromQuery] DateTime? uploadedBefore = null,
        [FromQuery] DateTime? uploadedAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.GetSingles(opts =>
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
        });

    /// <summary>
    /// Get many <see cref="Photo"/>'s singles (medium-images) matching a number of given criterias passed by URL/Query Parameters.
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
    [HttpGet("medium")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<Photo>>> GetMediumSingles(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? slug = null,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] int? uploadedBy = null,
        [FromQuery] DateTime? uploadedBefore = null,
        [FromQuery] DateTime? uploadedAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.GetSingles(opts =>
        {
            opts.Limit = limit;
            opts.Offset = offset;
            opts.Dimension = Dimension.MEDIUM;
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
    /// Get many <see cref="Photo"/>'s singles (thumbnail-images) matching a number of given criterias passed by URL/Query Parameters.
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
    [HttpGet("thumbnail")]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<Photo>>> GetThumbnailSingles(
        [Required] int limit = 99,
        [Required] int offset = 0,
        [FromQuery] string? slug = null,
        [FromQuery] string? title = null,
        [FromQuery] string? summary = null,
        [FromQuery] int? uploadedBy = null,
        [FromQuery] DateTime? uploadedBefore = null,
        [FromQuery] DateTime? uploadedAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] DateTime? createdAfter = null
    ) =>
        await handler.GetSingles(opts =>
        {
            opts.Limit = limit;
            opts.Offset = offset;
            opts.Dimension = Dimension.THUMBNAIL;
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
    [HttpGet]
    [Tags(ControllerTags.PHOTOS_ENTITIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(
        [Required] int limit = 99,
        [Required] int offset = 0,
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
        await handler.GetPhotos(opts =>
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [Tags(ControllerTags.PHOTOS_ENTITIES, ControllerTags.PHOTOS_FILES)]
    public async Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhotos(/*
        [FromQuery] string? title = null, // Does not support model binding, whatever that is.
        [FromQuery] string? summary = null,
        [FromQuery] string[]? tags = null
    */) =>
        await handler.UploadPhotos(/* opts => {
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
    public async Task<ActionResult<PhotoEntity>> UpdatePhoto(MutatePhoto mut) =>
        await handler.UpdatePhotoEntity(mut);
    #endregion
}
