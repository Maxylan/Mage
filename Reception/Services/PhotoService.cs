using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Reception.Authentication;
using Reception.Models;
using Reception.Models.Entities;
using Reception.Utilities;
using Reception.Interfaces;
using Reception.Controllers;
using System.Globalization;
using System.Text;
using System.Net;
using Reception.Constants;
using System.Linq.Expressions;

namespace Reception.Services;

public class PhotoService(
    MageDbContext db,
    ILoggingService logging,
    IHttpContextAccessor contextAccessor,
    ITagService tagService
) : IPhotoService
{
    #region Get base filepaths.
    public const string FILE_STORAGE_NAME = "Postbox";
    public static readonly Dictionary<Dimension, string> StorageDirectories = new() {
        { Dimension.SOURCE, "source" },
        { Dimension.MEDIUM, "medium" },
        { Dimension.THUMBNAIL, "thumbnail" }
    };

    /// <summary>
    /// Get the name (only) of the base directory of my file storage
    /// </summary>
    public string GetBaseDirectoryName() => FILE_STORAGE_NAME;
    /// <summary>
    /// Get the name (only) of the Thumbnail directory of my file storage
    /// </summary>
    public string GetThumbnailDirectoryName() => StorageDirectories[Dimension.THUMBNAIL];
    /// <summary>
    /// Get the name (only) of the Medium directory of my file storage
    /// </summary>
    public string GetMediumDirectoryName() => StorageDirectories[Dimension.MEDIUM];
    /// <summary>
    /// Get the name (only) of the Source directory of my file storage
    /// </summary>
    public string GetSourceDirectoryName() => StorageDirectories[Dimension.SOURCE];
    /// <summary>
    /// Get the path (directories, plural) to the directory relative to a <see cref="DateTime"/>
    /// </summary>
    public string GetDatePath(DateTime dateTime) => Path.Combine(
        dateTime.Year.ToString(),
        dateTime.Month.ToString(),
        dateTime.Day.ToString()
    );
    /// <summary>
    /// Get the <strong>combined</strong> relative path (<c>Base + Thumbnail/Medium/Source + DatePath</c>) to a directory in my file storage.
    /// </summary>
    public string GetCombinedPath(Dimension dimension, DateTime? dateTime = null, string filename = "") => Path.Combine(
        GetBaseDirectoryName(),
        dimension switch
        {
            Dimension.THUMBNAIL => GetThumbnailDirectoryName(),
            Dimension.MEDIUM => GetMediumDirectoryName(),
            Dimension.SOURCE => GetSourceDirectoryName(),
            _ => GetSourceDirectoryName()
        },
        GetDatePath(dateTime ?? DateTime.UtcNow),
        filename
    );
    #endregion


    #region Get single photos.
    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> GetPhotoEntity(int photoId)
    {
        if (photoId <= 0)
        {
            throw new ArgumentException($"Parameter {nameof(photoId)} has to be a non-zero positive integer!", nameof(photoId));
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} matching the given {nameof(photoId)} #{photoId}.";
            await logging
                .Action(nameof(GetPhotoEntity))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (photo.Filepaths is null || photo.Filepaths.Count == 0)
        {
            // Load missing navigation entries.
            foreach (var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded)
                {
                    await navigation.LoadAsync();
                }
            }
        }

        return photo;
    }

    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> GetPhotoEntity(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        PhotoEntity? photo = await db.Photos
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Links)
            .Include(photo => photo.Tags)
            .FirstOrDefaultAsync(photo => photo.Slug == slug);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} matching the given {nameof(slug)} '{slug}'.";
            await logging
                .Action(nameof(GetPhotoEntity))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return photo;
    }


    /// <summary>
    /// Get the <see cref="Reception.Models.Photo"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<Photo>> GetSinglePhoto(int photoId, Dimension dimension = Dimension.SOURCE)
    {
        var getEntity = await this.GetPhotoEntity(photoId);
        PhotoEntity? entity = getEntity.Value;

        if (entity is null || getEntity.Result is NotFoundObjectResult)
        {
            return getEntity.Result!;
        }

        if (!entity.Filepaths.Any(path => path.Dimension == dimension))
        {
            string message = $"Photo {nameof(PhotoEntity)} (#{photoId}) did not have a {dimension} {nameof(Dimension)}.";
            await logging
                .Action(nameof(GetSinglePhoto))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new Photo(entity, dimension);
    }

    /// <summary>
    /// Get the <see cref="Reception.Models.Photo"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<Photo>> GetSinglePhoto(string slug, Dimension dimension = Dimension.SOURCE)
    {
        var getEntity = await this.GetPhotoEntity(slug);
        PhotoEntity? entity = getEntity.Value;

        if (entity is null || getEntity.Result is NotFoundObjectResult)
        {
            return getEntity.Result!;
        }

        if (!entity.Filepaths.Any(path => path.Dimension == dimension))
        {
            string message = $"Photo {nameof(PhotoEntity)} ('{slug}') did not have a {dimension} {nameof(Dimension)}.";
            await logging
                .Action(nameof(GetSinglePhoto))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new Photo(entity, dimension);
    }


    /// <summary>
    /// Get the <see cref="PhotoCollection"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<PhotoCollection>> GetPhoto(int photoId)
    {
        if (photoId <= 0)
        {
            throw new ArgumentException($"Parameter {nameof(photoId)} has to be a non-zero positive integer!", nameof(photoId));
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} matching the given {nameof(photoId)} #{photoId}.";
            await logging
                .Action(nameof(GetPhoto))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (photo.Filepaths is null || photo.Filepaths.Count == 0)
        {
            // Load missing navigation entries.
            foreach (var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded)
                {
                    await navigation.LoadAsync();
                }
            }
        }

        if (!photo.Filepaths!.Any(path => path.Dimension == Dimension.SOURCE))
        {
            string message = $"{nameof(PhotoEntity)} '{photo.Slug}' didn't have a {Dimension.SOURCE} {nameof(Dimension)}";
            // throw new Exception(message);

            await logging
                .Action(nameof(GetPhoto))
                .InternalWarning(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new PhotoCollection(photo);
    }

    /// <summary>
    /// Get the <see cref="PhotoCollection"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<PhotoCollection>> GetPhoto(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        PhotoEntity? photo = await db.Photos
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Links)
            .Include(photo => photo.Tags)
            .FirstOrDefaultAsync(photo => photo.Slug == slug);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} matching the given {nameof(slug)} '{slug}'.";
            await logging
                .Action(nameof(GetPhoto))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (photo.Filepaths is null || photo.Filepaths.Count == 0)
        {
            // Load missing navigation entries.
            foreach (var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded)
                {
                    await navigation.LoadAsync();
                }
            }
        }

        if (!photo.Filepaths!.Any(path => path.Dimension == Dimension.SOURCE))
        {
            string message = $"{nameof(PhotoEntity)} '{photo.Slug}' didn't have a {Dimension.SOURCE} {nameof(Dimension)}";
            // throw new Exception(message);

            await logging
                .Action(nameof(GetPhoto))
                .InternalWarning(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new PhotoCollection(photo);
    }
    #endregion


    #region Get many photos.
    /// <summary>
    /// Get all <see cref="Reception.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public Task<ActionResult<IEnumerable<Photo>>> GetSingles(string? search, Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetSingles(search, filtering);
    }

    /// <summary>
    /// Get all <see cref="Reception.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<Photo>>> GetSingles(string? search, FilterPhotosOptions filter)
    {
        filter.Dimension ??= Dimension.SOURCE;

        IQueryable<PhotoEntity> photoQuery = db.Photos
            .OrderByDescending(photo => photo.CreatedAt)
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Links)
            .Include(photo => photo.Tags)
            .Where(photo => photo.Filepaths.Any(path => path.Dimension == filter.Dimension));

        // Searching (OR)
        if (!string.IsNullOrWhiteSpace(search))
        {
            photoQuery = photoQuery
                .Where(photo => (
                    photo.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || photo.Slug.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || photo.Summary.Contains(search, StringComparison.OrdinalIgnoreCase)
                ));
        }

        // Filtering (AND)
        if (!string.IsNullOrWhiteSpace(filter.Slug))
        {
            photoQuery = photoQuery
                .Where(photo => photo.Slug.Contains(filter.Slug));
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            if (!filter.Title.IsNormalized())
            {
                filter.Title = filter.Title
                    .Normalize()
                    .Trim();
            }

            photoQuery = photoQuery
                .Where(photo => !string.IsNullOrWhiteSpace(photo.Title))
                .Where(photo => photo.Title!.Contains(filter.Title));
        }

        if (!string.IsNullOrWhiteSpace(filter.Summary))
        {
            if (!filter.Summary.IsNormalized())
            {
                filter.Summary = filter.Summary
                    .Normalize()
                    .Trim();
            }

            photoQuery = photoQuery
                .Where(photo => !string.IsNullOrWhiteSpace(photo.Summary))
                .Where(photo => photo.Summary!.Contains(filter.Summary));
        }

        if (filter.UploadedBy is not null)
        {
            if (filter.UploadedBy <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.UploadedBy, $"Filter Parameter {nameof(filter.UploadedBy)} has to be a non-zero positive integer (User ID)!");
            }

            photoQuery = photoQuery
                .Where(photo => photo.UploadedBy == filter.UploadedBy);
        }

        if (filter.UploadedBefore is not null)
        {
            photoQuery = photoQuery
                .Where(photo => photo.UploadedAt <= filter.UploadedBefore);
        }
        else if (filter.UploadedAfter is not null)
        {
            if (filter.UploadedAfter > DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.UploadedAfter, $"Filter Parameter {nameof(filter.UploadedAfter)} cannot exceed DateTime.UtcNow");
            }

            photoQuery = photoQuery
                .Where(photo => photo.UploadedAt >= filter.UploadedAfter);
        }

        if (filter.CreatedBefore is not null)
        {
            photoQuery = photoQuery
                .Where(photo => photo.CreatedAt <= filter.CreatedBefore);
        }
        else if (filter.CreatedAfter is not null)
        {
            if (filter.CreatedAfter > DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedAfter, $"Filter Parameter {nameof(filter.CreatedAfter)} cannot exceed DateTime.UtcNow");
            }

            photoQuery = photoQuery
                .Where(photo => photo.CreatedAt >= filter.CreatedAfter);
        }

        if (filter.Tags is not null && filter.Tags.Length > 0)
        {
            var sanitizeAndCreateTags = await tagService.CreateTags(filter.Tags);
            Tag[]? validTags = sanitizeAndCreateTags.Value?.ToArray();

            if (validTags?.Any() == true)
            {
                photoQuery = photoQuery
                    .Where( // I really hope this makes a valid query..
                        photo => photo.Tags.Any(tag => validTags.Contains(tag))
                    );
            }
        }

        // Pagination
        if (filter.Offset is not null)
        {
            if (filter.Offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Offset, $"Pagination Parameter {nameof(filter.Offset)} has to be a positive integer!");
            }

            photoQuery = photoQuery.Skip(filter.Offset.Value);
        }
        if (filter.Limit is not null)
        {
            if (filter.Limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Limit, $"Pagination Parameter {nameof(filter.Limit)} has to be a non-zero positive integer!");
            }

            photoQuery = photoQuery.Take(filter.Limit.Value);
        }

        var getPhotos = await photoQuery.ToListAsync();
        var photos = getPhotos
            .Select(photo => new Photo(photo, filter.Dimension!.Value))
            .ToList();

        return photos;
    }


    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(string? search, Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(search, filtering);
    }

    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(string? search, FilterPhotosOptions filter)
    {
        IQueryable<PhotoEntity> photoQuery = db.Photos
            .OrderByDescending(photo => photo.CreatedAt)
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Links)
            .Include(photo => photo.Tags);

        // Searching (OR)
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Default search predicate/expression..
            Expression<Func<PhotoEntity, bool>> predicate = (PhotoEntity photo) => (
                photo.Title.StartsWith(search)
                || photo.Title.EndsWith(search)
                || photo.Slug.StartsWith(search)
                || photo.Slug.EndsWith(search)
                || photo.Filepaths.Any(path => (
                    path.Filename.StartsWith(search)
                    || path.Filename.EndsWith(search)
                ))
            );

            if (search.Length > 2) {
                // Permissive search predicate/expression (..used by search-queries exceeding two characters)..
                predicate = photo => (
                    photo.Title.ToUpper().Contains(search.ToUpper())
                    || photo.Slug.ToUpper().Contains(search.ToUpper())
                    || string.IsNullOrWhiteSpace(photo.Summary)
                    || photo.Summary.ToUpper().Contains(search.ToUpper())
                    || photo.Filepaths.Any(path => path.Filename.ToUpper().Contains(search.ToUpper()))
                );
            }

            photoQuery = photoQuery.Where(predicate);
        }

        // Filtering (AND)
        if (filter.Dimension is not null)
        {
            photoQuery = photoQuery
                .Where(photo => photo.Filepaths.Any(path => path.Dimension == filter.Dimension));
        }

        if (!string.IsNullOrWhiteSpace(filter.Slug))
        {
            photoQuery = photoQuery
                .Where(photo => photo.Slug.Contains(filter.Slug));
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            photoQuery = photoQuery
                .Where(photo => !string.IsNullOrWhiteSpace(photo.Title))
                .Where(photo => photo.Title!.Contains(filter.Title));
        }

        if (filter.UploadedBy is not null)
        {
            if (filter.UploadedBy <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.UploadedBy, $"Filter Parameter {nameof(filter.UploadedBy)} has to be a non-zero positive integer (User ID)!");
            }

            photoQuery = photoQuery
                .Where(photo => photo.UploadedBy == filter.UploadedBy);
        }

        if (filter.UploadedBefore is not null)
        {
            photoQuery = photoQuery
                .Where(photo => photo.UploadedAt <= filter.UploadedBefore);
        }
        else if (filter.UploadedAfter is not null)
        {
            if (filter.UploadedAfter > DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.UploadedAfter, $"Filter Parameter {nameof(filter.UploadedAfter)} cannot exceed DateTime.UtcNow");
            }

            photoQuery = photoQuery
                .Where(photo => photo.UploadedAt >= filter.UploadedAfter);
        }

        if (filter.CreatedBefore is not null)
        {
            photoQuery = photoQuery
                .Where(photo => photo.CreatedAt <= filter.CreatedBefore);
        }
        else if (filter.CreatedAfter is not null)
        {
            if (filter.CreatedAfter > DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedAfter, $"Filter Parameter {nameof(filter.CreatedAfter)} cannot exceed DateTime.UtcNow");
            }

            photoQuery = photoQuery
                .Where(photo => photo.CreatedAt >= filter.CreatedAfter);
        }

        // Pagination
        if (filter.Offset is not null)
        {
            if (filter.Offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Offset, $"Pagination Parameter {nameof(filter.Offset)} has to be a positive integer!");
            }

            photoQuery = photoQuery.Skip(filter.Offset.Value);
        }
        if (filter.Limit is not null)
        {
            if (filter.Limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Limit, $"Pagination Parameter {nameof(filter.Limit)} has to be a non-zero positive integer!");
            }

            photoQuery = photoQuery.Take(filter.Limit.Value);
        }

        var getPhotos = await photoQuery.ToListAsync();
        var photos = getPhotos
            .Select(entity => new PhotoCollection(entity))
            .ToList();

        return photos;
    }
    #endregion


    #region Create / Store photos.
    /// <summary>
    /// Upload any amount of new photos/files (<see cref="PhotoEntity"/>, <seealso cref="Reception.Models.Entities.PhotoCollection"/>)
    /// by streaming them directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as options/details of the
    /// generated database entitities.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhotos(Action<PhotosOptions> opts)
    {
        FilterPhotosOptions options = new();
        opts(options);

        return UploadPhotos(options);
    }

    /// <summary>
    /// Upload any amount of new photos/files (<see cref="PhotoEntity"/>, <seealso cref="Reception.Models.Entities.PhotoCollection"/>)
    /// by streaming them directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="options"/>) has been repurposed to serve as options/details of the
    /// generated database entitities.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public async Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhotos(PhotosOptions options)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(UploadPhotos)} Failed: No {nameof(HttpContext)} found.";
            await logging
                .Action(nameof(UploadPhotos))
                .InternalError(message)
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (!MultipartHelper.IsMultipartContentType(httpContext.Request.ContentType))
        {
            string message = $"{nameof(UploadPhotos)} Failed: Request couldn't be processed, not a Multipart Formdata request.";
            await logging
                .Action(nameof(UploadPhotos))
                .ExternalError(message)
                .SaveAsync();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        Account? user = null;

        if (MageAuthentication.IsAuthenticated(contextAccessor))
        {
            try
            {
                user = MageAuthentication.GetAccount(contextAccessor);
            }
            catch (Exception ex)
            {
                await logging
                    .Action(nameof(UploadPhotos))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .SaveAsync();
            }
        }

        if (user is not null)
        {
            options.UploadedBy = user.Id;
        }

        var mediaTypeHeader = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType!);
        string boundary = MultipartHelper.GetBoundary(mediaTypeHeader, 70);

        var reader = new MultipartReader(boundary, httpContext.Request.Body);
        MultipartSection? section;

        List<PhotoCollection> photos = [];
        uint iteration = 0u;
        do
        {
            section = await reader.ReadNextSectionAsync();
            if (section is null)
            {
                break;
            }

            bool hasContentDisposition =
                ContentDispositionHeaderValue.TryParse(section?.ContentDisposition, out var contentDisposition);

            if (!hasContentDisposition || contentDisposition is null)
            {
                continue;
            }

            if (MultipartHelper.HasFileContentDisposition(contentDisposition))
            {
                PhotoEntity? newPhoto;
                try
                {
                    newPhoto = await UploadSinglePhoto(
                        options,
                        contentDisposition,
                           section!,
                          user
                    );

                    if (newPhoto is null)
                    {
                        await logging
                            .Action(nameof(UploadPhotos))
                            .InternalError($"Failed to create a {nameof(PhotoEntity)} using uploaded photo. {nameof(newPhoto)} was null.")
                            .SaveAsync();
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    await logging
                         .Action(nameof(UploadPhotos))
                           .InternalError($"Failed to upload a photo. ({ex.GetType().Name}) " + ex.Message, opts =>
                           {
                               opts.Exception = ex;
                           })
                         .SaveAsync();
                    continue;
                }

                if (newPhoto.Id == default)
                {
                    // New photo needs to be uploaded to the database..
                    var createEntity = await CreatePhotoEntity(newPhoto);
                    newPhoto = createEntity.Value;

                    if (newPhoto is null || newPhoto.Id == default)
                    {
                        await logging
                            .Action(nameof(UploadPhotos))
                            .InternalError($"Failed to create a {nameof(PhotoEntity)} using uploaded photo '{(newPhoto?.Slug ?? "null")}'. Entity was null, or its Photo ID remained as 'default' post-saving to the database ({(newPhoto?.Id.ToString() ?? "null")}).")
                            .SaveAsync();
                        continue;
                    }
                }

                if (!newPhoto!.Filepaths.Any(path => path.IsSource))
                {
                    await logging
                        .Action(nameof(UploadPhotos))
                        .InternalError($"No '{Dimension.SOURCE.ToString()}' {nameof(Filepath)} found in the newly uploaded/created {nameof(PhotoEntity)} instance '{newPhoto.Slug}' (#{newPhoto.Id}).")
                        .SaveAsync();
                    continue;
                }

                photos.Add(
                    new(newPhoto)
                );
            }
            else if (MultipartHelper.HasFormDataContentDisposition(contentDisposition))
            {
                // TODO! Alternative way to get created-date?
                // if (string.IsNullOrWhiteSpace(contentDisposition.CreationDate))

                string sectionName = contentDisposition.Name.ToString().ToLower();
                if (sectionName == "title")
                {
                    string extractedTitle = await section!.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(extractedTitle))
                    {
                        options.Title = extractedTitle;
                    }

                    continue;
                }

                if (sectionName == "slug")
                {
                    string extractedSlug = await section!.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(extractedSlug))
                    {
                        options.Slug = extractedSlug;
                    }

                    continue;
                }

                if (sectionName == "tags")
                {
                    string tagsString = await section!.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(tagsString))
                    {
                        tagsString = tagsString.Replace(", ", ","); // Additional level of fault-tolerance..
                        options.Tags = tagsString
                            .Trim()
                            .Split(",")
                            .Where(_t => !string.IsNullOrWhiteSpace(_t))
                            .ToArray();
                    }

                    continue;
                }
            }
        }
        while (section is not null && ++iteration < 4096u);

        return new CreatedAtActionResult(null, null, null, photos);
    }

    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoEntity"/></returns>
    protected async Task<PhotoEntity> UploadSinglePhoto(
        PhotosOptions options,
        ContentDispositionHeaderValue contentDisposition,
        MultipartSection section,
        Account? user = null
    )
    {
        string sourcePath = string.Empty;
        string mediumPath = string.Empty;
        string thumbnailPath = string.Empty;
        string fileExtension = string.Empty;
        string trustedFilename = string.Empty;
        DateTime createdAt = DateTime.UtcNow; // Fallback in case no EXIF data..
        DateTime uploadedAt = DateTime.UtcNow;

        // Don't trust the file name sent by the client. To display the file name, HTML-encode the value.
        // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0#upload-large-files-with-streaming
        string? untrustedFilename = contentDisposition.FileName.Value;

        if (string.IsNullOrWhiteSpace(untrustedFilename))
        {
            throw new NotImplementedException("Handle case of no filename"); // TODO: HANDLE
        }

        if (!untrustedFilename.IsNormalized())
        {
            untrustedFilename = untrustedFilename
                .Normalize()
                .Trim();
        }

        trustedFilename = WebUtility.HtmlEncode(untrustedFilename);
        var filenameParts = trustedFilename.Split('.');
        string sanitizedName = string.Join("_", filenameParts[..^1])
            .Trim()
            .Replace(Path.DirectorySeparatorChar.ToString(), "\\" + Path.DirectorySeparatorChar)
            .Subsmart(0, 123);

        // Should be normalized, encoded, trimmed and clamped! The four avengers of trust
        trustedFilename = sanitizedName + "." + filenameParts[^1];

        if (trustedFilename.Contains("&&")
            || trustedFilename.Contains("..")
            || trustedFilename.Contains(Path.DirectorySeparatorChar)
            || trustedFilename.Length > 127
        )
        {
            throw new NotImplementedException("Suspicious upload? " + trustedFilename); // TODO! Handle!!
        }

        fileExtension = Path.GetExtension(trustedFilename).ToLowerInvariant();
        if (fileExtension.StartsWith('.') && fileExtension.Length > 1)
        {
            fileExtension = fileExtension[1..];
        }

        if (string.IsNullOrWhiteSpace(fileExtension) || !MimeVerifyer.SupportedExtensions.Contains(fileExtension))
        {
            throw new NotImplementedException("The extension is invalid.. discontinuing processing of the file"); // TODO: HANDLE
        }

        using MemoryStream requestStream = new MemoryStream();
        await section.Body.CopyToAsync(requestStream);

        if (requestStream.Length <= 0)
        {
            throw new NotImplementedException("The file is empty."); // TODO: HANDLE
        }
        if (requestStream.Length > MultipartHelper.FILE_SIZE_LIMIT)
        {
            throw new NotImplementedException("The file is too large ... discontinuing processing of the file"); // TODO: HANDLE
        }

        // Attempt to manually detect and return image format. Also calls `MimeVerifyer.ValidateContentType()`
        IImageFormat? format = MimeVerifyer.DetectImageFormat(trustedFilename, fileExtension, requestStream);

        if (format is null)
        {   // TODO! Handle below scenarios!
            // - Bad filename/extension string
            // - Extension/Filename missmatch
            // - MIME not supported
            // - MIME not a valid image type.
            // - MIME not supported by ImageSharp / Missing ImageFormat
            // - MIME Could not be validated (bad magic numbers)
            throw new NotImplementedException($"{nameof(UploadSinglePhoto)} {nameof(format)} is null."); // TODO! Handle!!
        }

        sourcePath = GetCombinedPath(Dimension.SOURCE, uploadedAt);
        mediumPath = GetCombinedPath(Dimension.MEDIUM, uploadedAt);
        thumbnailPath = GetCombinedPath(Dimension.THUMBNAIL, uploadedAt);
        // TODO! Parse EXIF before combining paths to use the date the picture was taken/created as its path, instead of the upload date.

        try
        {
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(mediumPath);
            Directory.CreateDirectory(thumbnailPath);
        }
        catch (Exception ex)
        {
            /* // Handle billions of different exceptions, maybe..
	        IOException
	        UnauthorizedAccessException
	        ArgumentException
	        ArgumentNullException
	        PathTooLongException
	        DirectoryNotFoundException
	        NotSupportedException
	        */
            throw new NotImplementedException($"Handle directory create errors {nameof(UploadSinglePhoto)} ({sourcePath}) " + ex.Message); // TODO: HANDLE
        }

        // Handle potential name-conflicts on the path..
        string filename = trustedFilename;
        string fullPath = Path.Combine(sourcePath, filename);
        int extensionIndex = trustedFilename.LastIndexOf('.');
        int conflicts = 0;

        while (File.Exists(fullPath) && ++conflicts <= 4096)
        {
            string appendix = "_copy";
            if (conflicts > 1)
            {
                appendix += "_" + (conflicts - 1);
            }

            filename = extensionIndex != -1
                ? trustedFilename.Insert(extensionIndex, appendix)
                 : (trustedFilename + appendix);

            fullPath = Path.Combine(sourcePath, filename);
        }

        // Saved! :)
        using FileStream file = File.Create(fullPath);
        await file.WriteAsync(requestStream.ToArray());
        await requestStream.DisposeAsync();
        file.Position = 0;

        long filesize = file.Length;
        long mediumFilesize = 0L;
        long thumbnailFilesize = 0L;
        Size sourceDimensions;
        Size mediumDimensions;
        Size thumbnailDimensions;
        string dpi = string.Empty;
        string sourceFilesizeFormatted = filesize.ToString();

        if (filesize < 8388608)
        { // Kilobytes..
            sourceFilesizeFormatted = $"{Math.Round((decimal)(filesize / 8192), 1)}kB";
        }
        else if (filesize >= 8589934592)
        { // Gigabytes..
            sourceFilesizeFormatted = $"{Math.Round((decimal)(filesize / 8589934592), 3)}GB";
        }
        else
        { // Megabytes..
            sourceFilesizeFormatted = $"{Math.Round((decimal)(filesize / 8388608), 2)}MB";
        }

        Configuration imageConfiguration = Configuration.Default.Clone();
        imageConfiguration.ReadOrigin = 0;

        DecoderOptions decoderOptions = new()
        {
            Configuration = imageConfiguration
        };

        // Acquire decoder/encoder(s) for the identified format..
        IImageDecoder imageDecoder = decoderOptions.Configuration.ImageFormatsManager.GetDecoder(format);
        if (imageDecoder is null)
        {
            throw new NotImplementedException($"{nameof(UploadSinglePhoto)} {nameof(imageDecoder)} is null."); // TODO! Handle!!
        }

        IImageEncoder imageEncoder = decoderOptions.Configuration.ImageFormatsManager.GetEncoder(format);
        if (imageEncoder is null)
        {
            throw new NotImplementedException($"{nameof(UploadSinglePhoto)} {nameof(imageEncoder)} is null."); // TODO! Handle!!
        }

        using (Image source = await imageDecoder.DecodeAsync(decoderOptions, file))
        {
            string alternatePath = fullPath;
            sourceDimensions = source.Size;

            var exifProfile = source.Metadata.ExifProfile;
            if (exifProfile is not null && exifProfile.TryGetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeOriginal, out var pictureTakenAt))
            {
                if (DateTime.TryParse(
                    pictureTakenAt.Value,
                    System.Globalization.CultureInfo.CurrentCulture,
                    DateTimeStyles.AdjustToUniversal,
                    out createdAt
                ))
                {
                    if (Program.IsDevelopment)
                    {
                        Console.WriteLine($"(Debug -> TryParse) Uploading image '{trustedFilename}' from (EXIF) '{pictureTakenAt.Value}' parsed as '{createdAt}'.");
                    }
                }
                else if (DateTime.TryParseExact(
                    pictureTakenAt.Value,
                    "yyyy:MM:dd HH:mm:ss", // Weird, example: 2024:07:20 15:12:48
                    System.Globalization.CultureInfo.CurrentCulture,
                    DateTimeStyles.AdjustToUniversal,
                    out createdAt
                ))
                {
                    if (Program.IsDevelopment)
                    {
                        Console.WriteLine($"(Debug -> TryParseExact) Uploading image '{trustedFilename}' from (EXIF) '{pictureTakenAt.Value}' parsed as '{createdAt}'.");
                    }
                }
            }

            double dpiX = Math.Round(source.Metadata.HorizontalResolution, 1);
            double dpiY = Math.Round(source.Metadata.VerticalResolution, 1);

            if (dpiX == default)
            {
                dpi = dpiY.ToString();
            }
            else if (dpiY == default)
            {
                dpi = dpiX.ToString();
            }
            else if (dpiX == dpiY)
            {
                dpi = dpiX.ToString();
            }
            else
            {
                dpi = $"{dpiX}x{dpiY}";
            }

            decimal aspectRatio = Math.Max(sourceDimensions.Width, sourceDimensions.Height) / Math.Min(sourceDimensions.Width, sourceDimensions.Height);

            mediumDimensions = new(
                (int)(Math.Clamp(ImageDimensions.Medium.TARGET * aspectRatio, ImageDimensions.Medium.CLAMP_MINIMUM, ImageDimensions.Medium.CLAMP_MAXIMUM)),
                (int)(Math.Clamp(ImageDimensions.Medium.TARGET * aspectRatio, ImageDimensions.Medium.CLAMP_MINIMUM, ImageDimensions.Medium.CLAMP_MAXIMUM))
            );

            if (sourceDimensions.Width > mediumDimensions.Width && sourceDimensions.Height > mediumDimensions.Height)
            {
                using Image medium = source.Clone(context =>
                {
                    context.Resize(new ResizeOptions()
                    {
                        Size = mediumDimensions,
                        Mode = ResizeMode.Max
                    });
                });

                mediumDimensions.Width = medium.Width;
                mediumDimensions.Height = medium.Height;
                alternatePath = Path.Combine(mediumPath, filename);

                using (var mediumFile = File.Create(alternatePath))
                {
                    await medium.SaveAsync(mediumFile, imageEncoder);
                    mediumFilesize = mediumFile.Length;
                    Console.WriteLine($"mediumFilesize {mediumFilesize}");
                }
            }

            thumbnailDimensions = new(
                (int)(Math.Clamp(ImageDimensions.Thumbnail.TARGET * aspectRatio, ImageDimensions.Thumbnail.CLAMP_MINIMUM, ImageDimensions.Thumbnail.CLAMP_MAXIMUM)),
                (int)(Math.Clamp(ImageDimensions.Thumbnail.TARGET * aspectRatio, ImageDimensions.Thumbnail.CLAMP_MINIMUM, ImageDimensions.Thumbnail.CLAMP_MAXIMUM))
            );

            if (sourceDimensions.Width > thumbnailDimensions.Width && sourceDimensions.Height > thumbnailDimensions.Height)
            {
                using Image thumbnail = source.Clone(context =>
                {
                    context.Resize(new ResizeOptions()
                    {
                        Size = thumbnailDimensions,
                        Mode = ResizeMode.Max
                    });
                });

                thumbnailDimensions.Width = thumbnail.Width;
                thumbnailDimensions.Height = thumbnail.Height;
                alternatePath = Path.Combine(thumbnailPath, filename);

                using (var thumbnailFile = File.Create(alternatePath))
                {
                    await thumbnail.SaveAsync(thumbnailFile, imageEncoder);
                    thumbnailFilesize = thumbnailFile.Length;
                    Console.WriteLine($"thumbnailFilesize {thumbnailFilesize}");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(options.Slug))
        {
            // `filename` is only guaranteed unique *for a single date (24h)*.
            options.Slug = $"{uploadedAt.ToShortDateString().Replace('/', '-')}-{filename}";

            extensionIndex = options.Slug.LastIndexOf('.');
            if (extensionIndex != -1)
            {
                options.Slug = options.Slug[..extensionIndex];
            }

            if (options.Slug.Length > 123)
            { // Limit length, to avoid hitting the limit of 128 with the auto-generated slug.
                options.Slug = options.Slug.Subsmart(0, 120) + "_" + options.Slug.Length;
            }

            // Resolve possible (..yet, unlikely) ..conflicts/duplicate slugs:
            int count = await db.Photos.CountAsync(photo => photo.Slug == options.Slug);
            if (count > 0)
            {
                options.Slug += "_" + count;
            }
        }
        else if (!options.Slug.IsNormalized())
        {
            options.Slug = options.Slug
                .Normalize()
                .Trim();
        }

        if (options.Slug.Length > 127)
        {
            string message = $"Failed to upload photo, {nameof(PhotoEntity.Slug)} exceeds maximum allowed length of 127.";
            await logging
                .Action(nameof(UploadSinglePhoto))
                .InternalWarning(message)
                .SaveAsync();

            throw new Exception(message); // TODO! Handle more gracefully? Maybe clean up saved image from disk as well.
        }

        if (string.IsNullOrWhiteSpace(options.Title))
        {
            options.Title = trustedFilename;
        }
        else if (!options.Title.IsNormalized())
        {
            options.Title = options.Title
                .Normalize()
                .Trim();
        }
        if (conflicts > 0)
        {
            options.Title += $" (#{conflicts})";
        }

        if (options.Title.Length > 255)
        {
            string message = $"Failed to upload photo, {nameof(PhotoEntity.Title)} exceeds maximum allowed length of 255.";
            await logging
                .Action(nameof(UploadSinglePhoto))
                .InternalWarning(message)
                .SaveAsync();

            throw new Exception(message); // TODO! Handle more gracefully? Maybe clean up saved image from disk as well.
        }

        if (string.IsNullOrWhiteSpace(options.Summary))
        {
            options.Summary = $"{options.Title} - ";
            if (sourceDimensions.Width != default && sourceDimensions.Height != default)
            {
                options.Summary += $"{sourceDimensions.Width}x{sourceDimensions.Height}, {sourceFilesizeFormatted}.";
            }
            else
            {
                options.Summary += $"{sourceFilesizeFormatted}.";
            }
        }
        else if (!options.Summary.IsNormalized())
        {
            options.Summary = options.Summary
                .Normalize()
                .Trim();
        }

        if (options.Summary.Length > 255)
        {   // Think this is the better option for summaries, since they're optional..
            options.Summary = options.Summary.Subsmart(0, 253) + "..";
        }

        StringBuilder formattedDescription = new();
        DateTime createdOrUploadedDate = createdAt != uploadedAt ? createdAt : uploadedAt;
        string createdOrUploadedText = createdAt != uploadedAt
            ? "Taken/Created "
            : "Uploaded ";

        formattedDescription.Append(createdOrUploadedText);

        formattedDescription.Append(createdOrUploadedDate.Month switch
        {
            1 => "January",
            2 => "February",
            3 => "March",
            4 => "April",
            5 => "May",
            6 => "June",
            7 => "July",
            8 => "August",
            9 => "September",
            10 => "October",
            11 => "November",
            12 => "December",
            _ => "Unknown"
        });

        formattedDescription.AppendFormat(" {0}", createdOrUploadedDate.Year);

        formattedDescription.AppendFormat(", saved to '{0}' (", sourcePath);

        if (sourceDimensions.Width != default && sourceDimensions.Height != default)
        {
            formattedDescription.AppendFormat("{0}x", sourceDimensions.Width);
            formattedDescription.AppendFormat("{0}, ", sourceDimensions.Height);
        }

        formattedDescription.AppendFormat("{0})", sourceFilesizeFormatted);

        if (conflicts > 0)
        {
            formattedDescription.AppendFormat(". Potentially a copy of {0} other files.", conflicts);
        }

        // TODO! Replace DB Call here with some tags service..

        List<Tag> tags = [];
        if (options.Tags is not null && options.Tags.Length > 0)
        {
            foreach (string tagName in options.Tags.Distinct())
            {
                Tag? tag = await db.Tags.FirstOrDefaultAsync(_t => _t.Name == tagName);

                if (tag is null)
                {
                    tag = new()
                    {
                        Name = tagName
                    };
                }

                tags.Add(tag);
            }
        }

        PhotoEntity photo = new()
        {
            Slug = options.Slug,
            Title = options.Title,
            Summary = options.Summary,
            Description = formattedDescription.ToString(),
            UploadedBy = options.UploadedBy,
            UploadedAt = uploadedAt,
            CreatedAt = createdAt,
            UpdatedAt = DateTime.UtcNow,
            Tags = tags,
            Filepaths = [
                new() {
                    Filename = filename,
                    Path = sourcePath,
                    Dimension = Dimension.SOURCE,
                    Filesize = filesize,
                    Height = sourceDimensions.Height,
                    Width = sourceDimensions.Width
                }
            ]
        };

        if (user is not null)
        {
            photo.UploadedBy = user.Id;
        }

        // Auto-generated Filepaths..
        if (mediumFilesize > 0)
        {
            photo.Filepaths.Add(new()
            {
                Filename = filename,
                Path = mediumPath,
                Dimension = Dimension.MEDIUM,
                Filesize = mediumFilesize,
                Height = mediumDimensions.Height,
                Width = mediumDimensions.Width
            });
        }

        if (thumbnailFilesize > 0)
        {
            photo.Filepaths.Add(new()
            {
                Filename = filename,
                Path = thumbnailPath,
                Dimension = Dimension.THUMBNAIL,
                Filesize = thumbnailFilesize,
                Height = thumbnailDimensions.Height,
                Width = thumbnailDimensions.Width
            });
        }

        // Auto-generated Tags..
        if (createdAt != uploadedAt)
        {
            // TODO! Replace DB Call here with some tags service..
            var year_tag = await db.Tags.FirstOrDefaultAsync(
                tag => tag.Name == createdAt.Year.ToString(System.Globalization.CultureInfo.CurrentCulture)
            );

            photo.Tags.Add(year_tag ?? new()
            {
                Name = createdAt.Year.ToString(System.Globalization.CultureInfo.CurrentCulture),
                Description = "Images taken/created during " + createdAt.Year.ToString(System.Globalization.CultureInfo.CurrentCulture)
            });
        }
        if (filesize >= MultipartHelper.LARGE_FILE_THRESHOLD)
        {
            // TODO! Replace DB Call here with some tags service..
            var hd_tag = await db.Tags.FirstOrDefaultAsync(
                tag => tag.Name == MultipartHelper.LARGE_FILE_CATEGORY_SLUG
            );

            photo.Tags.Add(hd_tag ?? new()
            {
                Name = MultipartHelper.LARGE_FILE_CATEGORY_SLUG,
                Description = "Large or High-Definition Images."
            });
        }
        else if (filesize < MultipartHelper.SMALL_FILE_THRESHOLD)
        {
            // TODO! Replace DB Call here with some tags service..
            var sd_tag = await db.Tags.FirstOrDefaultAsync(
                tag => tag.Name == MultipartHelper.SMALL_FILE_CATEGORY_SLUG
            );

            photo.Tags.Add(sd_tag ?? new()
            {
                Name = MultipartHelper.SMALL_FILE_CATEGORY_SLUG,
                Description = "Small or Low-Definition Images and/or thumbnails."
            });
        }
        if (conflicts > 0)
        {
            // TODO! Replace DB Call here with some tags service..
            var copy_tag = await db.Tags.FirstOrDefaultAsync(
                tag => tag.Name == "Copy"
            );

            photo.Tags.Add(copy_tag ?? new()
            {
                Name = "Copy",
                Description = "Image might be a copy of another, its filename conflicts with at least one other file uploaded around the same time."
            });
        }

        logging
            .Action(nameof(UploadSinglePhoto))
            .ExternalInformation($"Finished streaming file '{filename}' to '{sourcePath}' and generated {nameof(PhotoCollection)} '{photo.Slug}'.");

        // Reset provided 'options' to defaults.
        // Order matters, since we're uploading more than one file..
        options.Slug = null;
        options.Title = null;
        options.Tags = null;

        return photo;
    }
    #endregion

    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> CreatePhotoEntity(MutatePhoto mut)
    {
        PhotoEntity entity = (PhotoEntity)mut;

        if (mut.Tags is not null)
        {
            if (mut.Tags.Length > 9999)
            {
                mut.Tags = mut.Tags
                    .Take(9999)
                    .ToArray();
            }

            mut.Tags = mut.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Where(t => t.Length < 128)
                .ToArray();

            if (mut.Tags.Length > 0)
            {
                var sanitizeAndCreateTags = await tagService.CreateTags(mut.Tags);
                entity.Tags = sanitizeAndCreateTags.Value?.ToList() ?? [];
            }
        }

        return await CreatePhotoEntity(entity);
    }

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> CreatePhotoEntity(PhotoEntity entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Slug))
        {
            int sourceIndex = entity.Filepaths
              .ToList().FindIndex(path => path.IsSource);

            if (sourceIndex != -1)
            {
                string filename = entity.Filepaths.ElementAt(sourceIndex).Filename;
                entity.Slug = WebUtility.HtmlEncode(filename)
                    .ToLower()
                    .Replace(" ", "-")
                    .Replace(".", "-");
            }
            else if (string.IsNullOrWhiteSpace(entity.Title))
            {
                string message = $"Can't save bad {nameof(PhotoEntity)} (#{entity.Id}) to database, entity has no '{Dimension.SOURCE.ToString()}' {nameof(Filepath)} and both '{nameof(PhotoEntity.Slug)}' & '{nameof(PhotoEntity.Title)}' are null/omitted!";
                await logging
                   .Action(nameof(CreatePhotoEntity))
                   .ExternalWarning(message)
                   .SaveAsync();

                return new BadRequestObjectResult(message);
            }
            else
            {
                entity.Slug = WebUtility.HtmlEncode(entity.Title)
                    .ToLower()
                    .Replace(" ", "-")
                    .Replace(".", "-");

                bool exists = await db.Photos.AnyAsync(photo => photo.Slug == entity.Slug);
                if (exists)
                {
                    string message = $"{nameof(PhotoEntity)} with unique '{entity.Slug}' already exists!";
                    await logging
                       .Action(nameof(CreatePhotoEntity))
                       .ExternalWarning(message)
                       .SaveAsync();

                    return new ObjectResult(message)
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                }
            }
        }

        try
        {
            db.Add(entity);
            await db.SaveChangesAsync();

            if (entity.Id == default)
            {
                await db.Entry(entity).ReloadAsync();
            }
        }
        catch (DbUpdateConcurrencyException concurrencyException)
        {
            string message = string.Empty;
            bool exists = await db.Photos.ContainsAsync(entity);

            if (exists)
            {
                message = $"{nameof(PhotoEntity)} '{entity.Slug}' (#{entity.Id}) already exists!";
                await logging
                   .Action(nameof(CreatePhotoEntity))
                   .InternalError(message, opts =>
                   {
                       opts.Exception = concurrencyException;
                   })
                   .SaveAsync();

                return new ObjectResult(message)
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            message = $"Cought a {nameof(DbUpdateConcurrencyException)} while attempting to save '{entity.Slug}' (#{entity.Id}) to database! ";
            await logging
               .Action(nameof(CreatePhotoEntity))
               .InternalError(message + concurrencyException.Message, opts =>
               {
                   opts.Exception = concurrencyException;
               })
                .SaveAsync();

            return new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {updateException.GetType().Name} while attempting to save '{entity.Slug}' (#{entity.Id}) to database! ";
            await logging
               .Action(nameof(CreatePhotoEntity))
               .InternalError(message + " " + updateException.Message, opts =>
               {
                   opts.Exception = updateException;
               })
                .SaveAsync();

            return new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return entity;
    }
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.PhotoEntity"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(MutatePhoto mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));
        ArgumentNullException.ThrowIfNull(mut.Id, nameof(mut.Id));

        if (mut.Tags?.Length > 9999)
        {
            mut.Tags = mut.Tags
                .Take(9999)
                .ToArray();
        }

        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(UpdatePhotoEntity)} Failed: No {nameof(HttpContext)} found.";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalError(message)
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        Account? user = null;

        if (MageAuthentication.IsAuthenticated(contextAccessor))
        {
            try
            {
                user = MageAuthentication.GetAccount(contextAccessor);
            }
            catch (Exception ex)
            {
                await logging
                    .Action(nameof(UpdatePhotoEntity))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .SaveAsync();
            }
        }

        if (mut.Id <= 0)
        {
            string message = $"Parameter '{nameof(mut.Id)}' has to be a non-zero positive integer! (Album ID)";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        PhotoEntity? existingPhoto = await db.Photos.FindAsync(mut.Id);

        if (existingPhoto is null)
        {
            string message = $"{nameof(Album)} with ID #{mut.Id} could not be found!";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .SaveAsync();

            return new NotFoundObjectResult(message);
        }

        foreach (var navigation in db.Entry(existingPhoto).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        if (string.IsNullOrWhiteSpace(mut.Slug))
        {
            string message = $"Parameter '{nameof(mut.Slug)}' may not be null/empty!";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        if (!mut.Slug.IsNormalized())
        {
            mut.Slug = mut.Slug
                .Normalize()
                .Trim();
        }
        if (mut.Slug.Length > 127)
        {
            string message = $"{nameof(PhotoEntity.Slug)} exceeds maximum allowed length of 127.";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        if (mut.Slug != existingPhoto.Slug)
        {
            bool slugTaken = await db.Photos.AnyAsync(photo => photo.Slug == mut.Slug);
            if (slugTaken)
            {
                string message = $"{nameof(PhotoEntity.Slug)} was already taken!";
                await logging
                    .Action(nameof(UpdatePhotoEntity))
                    .InternalDebug(message, opts =>
                    {
                        opts.SetUser(user);
                    })
                    .SaveAsync();

                return new ObjectResult(message)
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }
        }

        if (string.IsNullOrWhiteSpace(mut.Title))
        {
            string message = $"Parameter '{nameof(mut.Title)}' may not be null/empty!";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        if (!mut.Title.IsNormalized())
        {
            mut.Title = mut.Title
                .Normalize()
                .Trim();
        }
        if (mut.Title.Length > 255)
        {
            string message = $"{nameof(PhotoEntity.Title)} exceeds maximum allowed length of 255.";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        if (!string.IsNullOrWhiteSpace(mut.Summary))
        {
            if (!mut.Summary.IsNormalized())
            {
                mut.Summary = mut.Summary
                    .Normalize()
                    .Trim();
            }
            if (mut.Summary.Length > 255)
            {
                string message = $"{nameof(PhotoEntity.Summary)} exceeds maximum allowed length of 255.";
                await logging
                    .Action(nameof(UpdatePhotoEntity))
                    .InternalDebug(message, opts =>
                    {
                        opts.SetUser(user);
                    })
                    .SaveAsync();

                return new BadRequestObjectResult(message);
            }
        }

        if (!string.IsNullOrWhiteSpace(mut.Description) && !mut.Description.IsNormalized())
        {
            mut.Description = mut.Description
                .Normalize()
                .Trim();
        }

        List<Tag>? validTags = null;
        if (mut.Tags?.Any() == true)
        {
            var sanitizeAndCreateTags = await tagService.CreateTags(mut.Tags);
            validTags = sanitizeAndCreateTags.Value?.ToList();
        }

        existingPhoto.Slug = mut.Slug;
        existingPhoto.Title = mut.Title;
        existingPhoto.Summary = mut.Summary;
        existingPhoto.Description = mut.Description;
        existingPhoto.UpdatedAt = DateTime.UtcNow;

        if (mut.Tags is not null)
        {
            existingPhoto.Tags = validTags ?? [];
        }

        try
        {
            db.Update(existingPhoto);

            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalInformation($"{nameof(PhotoEntity)} '{existingPhoto.Slug}' (#{existingPhoto.Id}) was just updated.", opts =>
                {
                    opts.SetUser(user);
                });

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update existing Album '{existingPhoto.Slug}'. ";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update existing Album '{existingPhoto.Slug}'. ";
            await logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return existingPhoto;
    }


    /// <summary>
    /// Removes a <see cref="Reception.Models.Entities.Tag"/> (..identified by <paramref name="tag"/>) from the
    /// <see cref="Reception.Models.Entities.PhotoEntity"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult> RemoveTag(int photoId, string tag)
    {
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (string.IsNullOrWhiteSpace(tag))
        {
            string message = $"Parameter '{nameof(tag)}' was null/empty!";
            await logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            await logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        PhotoEntity? existingPhoto = await db.Photos
            .Include(photo => photo.Tags)
            .FirstOrDefaultAsync(photo => photo.Id == photoId);

        if (existingPhoto is null)
        {
            string message = $"{nameof(PhotoEntity)} with ID #{photoId} could not be found!";
            await logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(message);
        }

        Tag? tagToRemove = existingPhoto.Tags
            .FirstOrDefault(t => t.Name == tag);

        if (tagToRemove is null)
        {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        existingPhoto.Tags.Remove(tagToRemove);

        try
        {
            db.Update(existingPhoto);

            logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalTrace($"A tag was just removed from {nameof(PhotoEntity)} ('{existingPhoto.Title}', #{existingPhoto.Id})");

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to remove a tag from Photo '{existingPhoto.Title}'. ";
            await logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to remove a tag from Photo '{existingPhoto.Title}'. ";
            await logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return new OkResult();
    }
    #endregion


    #region Delete a photo completely (blob, filepaths & photo)
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            await logging
                .Action(nameof(DeletePhoto))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"{nameof(PhotoEntity)} with ID #{photoId} could not be found!";
            await logging
                .Action(nameof(DeletePhoto))
                .InternalDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(message);
        }

        return await DeletePhoto(photo);
    }
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult> DeletePhoto(PhotoEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        foreach (var navigation in db.Entry(entity).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        foreach (var path in entity.Filepaths)
        {
            var deleteBlobResult = await DeletePhotoBlob(path);

            if (deleteBlobResult is not NoContentResult)
            {
                return deleteBlobResult;
            }
        }

        return await DeletePhotoEntity(entity);
    }
    #endregion


    #region Delete a blob from disk
    /// <summary>
    /// Deletes the blob of a <see cref="Reception.Models.Entities.Photo"/> from disk.
    /// </summary>
    public async Task<ActionResult> DeletePhotoBlob(Filepath entity)
    {
        string fullPath = Path.Combine(entity.Path, entity.Filename);
        if (fullPath.Contains("&&") || fullPath.Contains("..") || fullPath.Length > 511)
        {
            await logging
                .Action(nameof(DeletePhotoBlob))
                .ExternalSuspicious($"Sussy filpath '{fullPath}' (TODO! HANDLE)")
                .SaveAsync();

            throw new NotImplementedException("Suspicious?"); // TODO! Handle!!
        }

        if (!File.Exists(fullPath))
        {
            string message = string.Empty;

            if (!fullPath.Contains(FILE_STORAGE_NAME))
            {
                message = $"Suspicious! Attempt was made to delete missing File '{fullPath}'! Is there a broken database entry, or did someone manage to escape a path string?";
                logging.Action(nameof(DeletePhotoBlob));
            }
            else
            {
                message = $"Attempt to delete File '{fullPath}' failed (file missing)! Assuming there's a dangling database entry..";
                logging.Action("Dangle -" + nameof(DeletePhotoBlob));
                // TODO! Automatically delete dangling entity?
                // Would need access to `PhotoEntity.Id` or slug here..
            }

            await logging
                .ExternalSuspicious(message)
                .SaveAsync();

            if (Program.IsProduction)
            {
                return new NotFoundResult();
            }

            return new NotFoundObjectResult(message);
        }

        try
        {
            File.Delete(fullPath);

            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalInformation($"The blob on path '{fullPath}' (Filepath ID #{entity.Id}) was just deleted.");
        }
        /* // TODO! Handle a gazillion different possible errors.
            ArgumentException
            ArgumentNullException
            DirectoryNotFoundException
            IOException
            NotSupportedException
            PathTooLongException
            UnauthorizedAccessException
        */
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete the blob on filepath '{fullPath}' (#{entity.Id}). ";
            await logging
                .Action(nameof(DeletePhotoEntity))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return new NoContentResult();
    }
    #endregion


    #region Delete photo entities from the database
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.PhotoEntity"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public async Task<ActionResult> DeletePhotoEntity(int photoId)
    {
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            await logging
                .Action(nameof(DeletePhotoEntity))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"{nameof(PhotoEntity)} with ID #{photoId} could not be found!";
            await logging
                .Action(nameof(DeletePhotoEntity))
                .InternalDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(message);
        }

        return await DeletePhotoEntity(photo);
    }

    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.PhotoEntity"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public async Task<ActionResult> DeletePhotoEntity(PhotoEntity entity)
    {
        try
        {
            db.Remove(entity);

            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalInformation($"The {nameof(PhotoEntity)} ('{entity.Title}', #{entity.Id}) was just deleted.");

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(PhotoEntity)} '{entity.Title}'. ";
            await logging
                .Action(nameof(DeletePhotoEntity))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(PhotoEntity)} '{entity.Title}'. ";
            await logging
                .Action(nameof(DeletePhotoEntity))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return new NoContentResult();
    }
    #endregion
}
