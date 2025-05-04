using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Mvc;
using Reception.Interfaces;
using Reception.Models;
using Reception.Models.Entities;
using Reception.Utilities;

namespace Reception.Services;

public class BlobService(
    ILoggingService<BlobService> logging,
    IPhotoService photoService
) : IBlobService
{
    /// <summary>
    /// Get the source blob associated with the <see cref="PhotoEntity"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public async Task<ActionResult> GetSourceBlobBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            Console.WriteLine($"{nameof(GetSourceBlobBySlug)} TODO! HANDLE!");
            return new BadRequestResult(); // TODO! Log & Handle..
        }

        var getSourcePhoto = await photoService.GetSinglePhoto(slug, Dimension.SOURCE);
        var source = getSourcePhoto.Value;
        if (source is null)
        {
            return getSourcePhoto.Result!;
        }
        else if (source.Filepath is null)
        {
            Console.WriteLine($"{nameof(GetSourceBlob)} TODO! HANDLE!");
            return new NotFoundResult(); // TODO! Log & Handle..
        }

        return await GetBlobAsync(source);
    }
    /// <summary>
    /// Get the source blob associated with the <see cref="PhotoEntity"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult> GetSourceBlob(int photoId)
    {
        var getSourcePhoto = await photoService.GetSinglePhoto(photoId, Dimension.SOURCE);
        var source = getSourcePhoto.Value;
        if (source is null)
        {
            return getSourcePhoto.Result!;
        }
        else if (source.Filepath is null)
        {
            Console.WriteLine($"{nameof(GetSourceBlob)} TODO! HANDLE!");
            return new NotFoundResult(); // TODO! Log & Handle..
        }

        return await GetBlobAsync(source);
    }
    /// <summary>
    /// Get the source blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public Task<ActionResult> GetSourceBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(Photo));
        ArgumentNullException.ThrowIfNull(photo.Filepath, nameof(Photo.Filepath));

        if (photo.Dimension != Dimension.SOURCE) {
            throw new ArgumentException($"Incorrect dimension ('{photo.Dimension.ToString()}') in given photo '{photo.Slug}' (#{photo.PhotoId})! Expected dimension '{Dimension.SOURCE.ToString()}'");
        }

        return GetBlobAsync(photo);
    }


    /// <summary>
    /// Get the medium blob associated with the <see cref="PhotoEntity"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public async Task<ActionResult> GetMediumBlobBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            Console.WriteLine($"{nameof( GetMediumBlobBySlug)} TODO! HANDLE!");
            return new BadRequestResult(); // TODO! Log & Handle..
        }

        var getMediumPhoto = await photoService.GetSinglePhoto(slug, Dimension.MEDIUM);
        var medium = getMediumPhoto.Value;
        if (medium is null)
        {
            return getMediumPhoto.Result!;
        }
        else if (medium.Filepath is null)
        {
            Console.WriteLine($"{nameof(GetMediumBlob)} TODO! HANDLE!");
            return new NotFoundResult(); // TODO! Log & Handle..
        }

        return await GetBlobAsync(medium);
    }
    /// <summary>
    /// Get the medium blob associated with the <see cref="PhotoEntity"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult> GetMediumBlob(int photoId)
    {
        var getMediumPhoto = await photoService.GetSinglePhoto(photoId, Dimension.MEDIUM);
        var medium = getMediumPhoto.Value;
        if (medium is null)
        {
            return getMediumPhoto.Result!;
        }
        else if (medium.Filepath is null)
        {
            Console.WriteLine($"{nameof(GetMediumBlob)} TODO! HANDLE!");
            return new NotFoundResult(); // TODO! Log & Handle..
        }

        return await GetBlobAsync(medium);
    }
    /// <summary>
    /// Get the medium blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public Task<ActionResult> GetMediumBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(Photo));
        ArgumentNullException.ThrowIfNull(photo.Filepath, nameof(Photo.Filepath));

        if (photo.Dimension != Dimension.MEDIUM) {
            throw new ArgumentException($"Incorrect dimension ('{photo.Dimension.ToString()}') in given photo '{photo.Slug}' (#{photo.PhotoId})! Expected dimension '{Dimension.MEDIUM.ToString()}'");
        }

        return GetBlobAsync(photo);
    }


    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="PhotoEntity"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public async Task<ActionResult> GetThumbnailBlobBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            Console.WriteLine($"{nameof(GetThumbnailBlobBySlug)} TODO! HANDLE!");
            return new BadRequestResult(); // TODO! Log & Handle..
        }

        var getThumbnailPhoto = await photoService.GetSinglePhoto(slug, Dimension.THUMBNAIL);
        var thumbnail = getThumbnailPhoto.Value;
        if (thumbnail is null)
        {
            return getThumbnailPhoto.Result!;
        }
        else if (thumbnail.Filepath is null)
        {
            Console.WriteLine($"{nameof(GetThumbnailBlob)} TODO! HANDLE!");
            return new NotFoundResult(); // TODO! Log & Handle..
        }

        return await GetBlobAsync(thumbnail);
    }
    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="PhotoEntity"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult> GetThumbnailBlob(int photoId)
    {
        var getThumbnailPhoto = await photoService.GetSinglePhoto(photoId, Dimension.THUMBNAIL);
        var thumbnail = getThumbnailPhoto.Value;
        if (thumbnail is null)
        {
            return getThumbnailPhoto.Result!;
        }
        else if (thumbnail.Filepath is null)
        {
            Console.WriteLine($"{nameof(GetThumbnailBlob)} TODO! HANDLE!");
            return new NotFoundResult(); // TODO! Log & Handle..
        }

        return await GetBlobAsync(thumbnail);
    }
    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public Task<ActionResult> GetThumbnailBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(Photo));
        ArgumentNullException.ThrowIfNull(photo.Filepath, nameof(Photo.Filepath));

        if (photo.Dimension != Dimension.THUMBNAIL) {
            throw new ArgumentException($"Incorrect dimension ('{photo.Dimension.ToString()}') in given photo '{photo.Slug}' (#{photo.PhotoId})! Expected dimension '{Dimension.THUMBNAIL.ToString()}'");
        }

        return GetBlobAsync(photo);
    }


    /// <summary>
    /// Get the blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public ActionResult GetBlob(Dimension dimension, PhotoEntity entity) =>
        this.GetBlob(new Photo(entity, dimension));


    /// <summary>
    /// Get the blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public ActionResult GetBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(Photo));
        ArgumentNullException.ThrowIfNull(photo.Filepath, nameof(Photo.Filepath));

        string path = Path.Combine(photo.Path, photo.Filename);
        try
        {
            FileStream fileStream = System.IO.File.OpenRead(path);
            IImageFormat? format = MimeVerifyer.DetectImageFormat(photo.Filename, fileStream);

            if (format is null)
            {   // TODO! Handle below scenarios!
                // - Bad filename/extension string
                // - Extension/Filename missmatch
                // - MIME not supported
                // - MIME not a valid image type.
                // - MIME not supported by ImageSharp / Missing ImageFormat
                // - MIME Could not be validated (bad magic numbers)
                throw new NotImplementedException($"{nameof(GetBlob)} ({photo.Dimension.ToString()}) {nameof(format)} is null."); // TODO! Handle!!
                // ..could fallback to "application/octet-stream here" instead of throwing?
            }

            fileStream.Position = 0;
            return new FileStreamResult(fileStream, format.DefaultMimeType);
        }
        catch (FileNotFoundException notFound)
        {
            string message = $"Cought a '{nameof(FileNotFoundException)}' attempting to open file " + (
                Program.IsDevelopment ? $"'{path}'. {notFound.Message}" : photo.Filename
            );

            logging
                .Action(nameof(GetBlob) + $" ({photo.Dimension.ToString()})")
                .InternalWarning(message, opts =>
                {
                    opts.Exception = notFound;
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }
        catch (UnauthorizedAccessException unauthorizedAccess)
        {
            string message = $"Cought {nameof(UnauthorizedAccessException)} attempting to open file '{path}'. {unauthorizedAccess.Message}";

            logging
                .Action(nameof(GetBlob) + $" ({photo.Dimension.ToString()})")
                .InternalError(message, opts =>
                {
                    opts.Exception = unauthorizedAccess;
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsDevelopment ? message : $"Failed to access '${photo.Filename}'") {
                StatusCode = StatusCodes.Status423Locked
            };
        }
        catch (Exception ex)
        {
            string message = "Internal Server Error";
            if (Program.IsDevelopment) {
                message += $" ({ex.GetType().Name}): {ex.Message}";
            }

            logging
                .Action(nameof(GetBlob) + $" ({photo.Dimension.ToString()})")
                .InternalError(message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return new ObjectResult(message) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }


    /// <summary>
    /// Get the blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public Task<ActionResult> GetBlobAsync(Dimension dimension, PhotoEntity entity) =>
        this.GetBlobAsync(new Photo(entity, dimension));

    /// <summary>
    /// Asynchronoushly get the blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public Task<ActionResult> GetBlobAsync(Photo photo)
    {
        return Task.Run(() => this.GetBlob(photo));
    }
}
