using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Mvc;
using Reception.Interfaces;
using Reception.Interfaces.DataAccess;
using Reception.Database;
using Reception.Database.Models;
using Reception.Utilities;
using Reception.Middleware.Authentication;
using System.Net;

namespace Reception.Services;

public class BlobService(
    ILoggingService<BlobService> logging,
    IHttpContextAccessor contextAccessor,
    IPhotoService photoService
) : IBlobService
{
    /// <summary>
    /// Get the source blob associated with the <see cref="Photo"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public async Task<ActionResult> GetSourceBlobBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            Console.WriteLine($"{nameof(GetSourceBlobBySlug)} TODO! HANDLE!");
            return new BadRequestResult(); // TODO! Log & Handle..
        }

        var getSourcePhoto = await photoService.GetPhoto(slug);
        var source = getSourcePhoto.Value;
        if (source is null)
        {
            return getSourcePhoto.Result!;
        }

        return await GetBlobAsync(source, Dimension.SOURCE);
    }
    /// <summary>
    /// Get the source blob associated with the <see cref="Photo"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult> GetSourceBlob(int photoId)
    {
        var getSourcePhoto = await photoService.GetPhoto(photoId);
        var source = getSourcePhoto.Value;
        if (source is null)
        {
            return getSourcePhoto.Result!;
        }

        return await GetBlobAsync(source, Dimension.SOURCE);
    }
    /// <summary>
    /// Get the source blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    public Task<ActionResult> GetSourceBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(Photo));
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(Photo.Filepaths));

        if (!photo.SourceExists) {
            throw new ArgumentException(
                $"Incorrect dimension(s) {nameof(Dimension)} in given photo '{photo.Slug}' (#{photo.Id})! Expected dimension '{Dimension.SOURCE.ToString()}'"
            );
        }

        return GetBlobAsync(photo, Dimension.SOURCE);
    }


    /// <summary>
    /// Get the medium blob associated with the <see cref="Photo"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public async Task<ActionResult> GetMediumBlobBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            Console.WriteLine($"{nameof( GetMediumBlobBySlug)} TODO! HANDLE!");
            return new BadRequestResult(); // TODO! Log & Handle..
        }

        var getMediumPhoto = await photoService.GetPhoto(slug);
        var medium = getMediumPhoto.Value;
        if (medium is null)
        {
            return getMediumPhoto.Result!;
        }

        return await GetBlobAsync(medium, Dimension.MEDIUM);
    }
    /// <summary>
    /// Get the medium blob associated with the <see cref="Photo"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult> GetMediumBlob(int photoId)
    {
        var getMediumPhoto = await photoService.GetPhoto(photoId);
        var medium = getMediumPhoto.Value;
        if (medium is null)
        {
            return getMediumPhoto.Result!;
        }

        return await GetBlobAsync(medium, Dimension.MEDIUM);
    }
    /// <summary>
    /// Get the medium blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    public Task<ActionResult> GetMediumBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(Photo));
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(Photo.Filepaths));

        if (!photo.MediumExists) {
            throw new ArgumentException(
                $"Incorrect dimension(s) {nameof(Dimension)} in given photo '{photo.Slug}' (#{photo.Id})! Expected dimension '{Dimension.MEDIUM.ToString()}'"
            );
        }

        return GetBlobAsync(photo, Dimension.MEDIUM);
    }


    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="Photo"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public async Task<ActionResult> GetThumbnailBlobBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            Console.WriteLine($"{nameof(GetThumbnailBlobBySlug)} TODO! HANDLE!");
            return new BadRequestResult(); // TODO! Log & Handle..
        }

        var getThumbnailPhoto = await photoService.GetPhoto(slug);
        var thumbnail = getThumbnailPhoto.Value;
        if (thumbnail is null)
        {
            return getThumbnailPhoto.Result!;
        }

        return await GetBlobAsync(thumbnail, Dimension.THUMBNAIL);
    }
    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="Photo"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult> GetThumbnailBlob(int photoId)
    {
        var getThumbnailPhoto = await photoService.GetPhoto(photoId);
        var thumbnail = getThumbnailPhoto.Value;
        if (thumbnail is null)
        {
            return getThumbnailPhoto.Result!;
        }

        return await GetBlobAsync(thumbnail, Dimension.THUMBNAIL);
    }
    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    public Task<ActionResult> GetThumbnailBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(Photo));
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(Photo.Filepaths));

        if (!photo.ThumbnailExists) {
            throw new ArgumentException(
                $"Incorrect dimension(s) {nameof(Dimension)} in given photo '{photo.Slug}' (#{photo.Id})! Expected dimension '{Dimension.THUMBNAIL.ToString()}'"
            );
        }

        return GetBlobAsync(photo, Dimension.THUMBNAIL);
    }


    /// <summary>
    /// Get the blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public ActionResult GetBlob(Photo photo, Dimension dimension)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(Photo));
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(Photo.Filepaths));

        Account? user;
        try
        {
            user = MemoAuth.GetAccount(contextAccessor);

            if (user is null) {
                return new ObjectResult("Prevented attempted unauthorized access.") {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
        catch (Exception ex)
        {
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(GetBlob))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredViewPrivilege = (byte)
            (photo.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL));

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(GetBlob))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        Filepath? filepath = photo.Filepaths.First(path => path.Dimension == dimension);
        if (filepath is null) {
            throw new ArgumentException(
                $"A filepath with {nameof(Dimension)} '{Dimension.THUMBNAIL.ToString()}' does not exist in the given photo '{photo.Slug}' (#{photo.Id})!"
            );
        }

        string path = Path.Combine(filepath.Path, filepath.Filename);
        try
        {
            FileStream fileStream = System.IO.File.OpenRead(path);
            IImageFormat? format = MimeVerifyer.DetectImageFormat(filepath.Filename, fileStream);

            if (format is null)
            {   // TODO! Handle below scenarios!
                // - Bad filename/extension string
                // - Extension/Filename missmatch
                // - MIME not supported
                // - MIME not a valid image type.
                // - MIME not supported by ImageSharp / Missing ImageFormat
                // - MIME Could not be validated (bad magic numbers)
                throw new NotImplementedException($"{nameof(GetBlob)} ({dimension.ToString()}) {nameof(format)} is null."); // TODO! Handle!!
                // ..could fallback to "application/octet-stream here" instead of throwing?
            }

            fileStream.Position = 0;
            return new FileStreamResult(fileStream, format.DefaultMimeType);
        }
        catch (FileNotFoundException notFound)
        {
            string message = $"Cought a '{nameof(FileNotFoundException)}' attempting to open file " + (
                Program.IsDevelopment ? $"'{path}'. {notFound.Message}" : filepath.Filename
            );

            logging
                .Action(nameof(GetBlob) + $" ({dimension.ToString()})")
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
                .Action(nameof(GetBlob) + $" ({dimension.ToString()})")
                .InternalError(message, opts =>
                {
                    opts.Exception = unauthorizedAccess;
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsDevelopment ? message : $"Failed to access '${filepath.Filename}'") {
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
                .Action(nameof(GetBlob) + $" ({dimension.ToString()})")
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
    /// Asynchronously get the blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public Task<ActionResult> GetBlobAsync(Photo entity, Dimension dimension)
    {
        return Task.Run(() => this.GetBlob(entity, dimension));
    }
}
