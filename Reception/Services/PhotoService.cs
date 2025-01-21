
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using PhotoEntity = Reception.Models.Entities.Photo;
using Photo = Reception.Models.Photo;
using Reception.Models;
using Reception.Interfaces;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Reception.Utilities;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;

namespace Reception.Services;

public class PhotoService(
    MageDbContext db,
    ILoggingService logging,
    IHttpContextAccessor contextAccessor
) : IPhotoService
{
    #region Get single photos.
    /// <summary>
    /// Get the <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> GetPhotoEntity(int photoId)
    {
        if (photoId <= 0) {
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
            foreach(var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded) {
                    await navigation.LoadAsync();
                }
            }
        }

        return photo;
    }

    /// <summary>
    /// Get the <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> GetPhotoEntity(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        PhotoEntity? photo = await db.Photos
            .Include(photo => photo.Filepaths)
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
        if (photoId <= 0) {
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
            foreach(var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded) {
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
            foreach(var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded) {
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
    public Task<ActionResult<IEnumerable<Photo>>> GetSingles(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetSingles(filtering);
    }

    /// <summary>
    /// Get all <see cref="Reception.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<Photo>>> GetSingles(FilterPhotosOptions filter)
    {
        filter.Dimension ??= Dimension.SOURCE;

        IQueryable<PhotoEntity> photoQuery = db.Photos
            .OrderByDescending(photo => photo.CreatedAt)
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Tags)
            .Where(photo => photo.Filepaths.Any(path => path.Dimension == filter.Dimension));

        // Filtering
        if (!string.IsNullOrWhiteSpace(filter.Slug))
        {
            photoQuery = photoQuery
                .Where(photo => photo.Slug.StartsWith(filter.Slug) || photo.Slug.EndsWith(filter.Slug));
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            photoQuery = photoQuery
                .Where(photo => !string.IsNullOrWhiteSpace(photo.Title))
                .Where(photo => photo.Title!.StartsWith(filter.Title) || photo.Title.EndsWith(filter.Title));
        }

        if (filter.CreatedAt is not null)
        {
            if (filter.CreatedAt > DateTime.UtcNow) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedAt, $"Filter Parameter {nameof(filter.CreatedAt)} cannot exceed DateTime.UtcNow");
            }
            
            photoQuery = photoQuery
                .Where(photo => photo.CreatedAt >= filter.CreatedAt);
        }

        if (filter.CreatedBy is not null)
        {
            if (filter.CreatedBy <= 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedBy, $"Filter Parameter {nameof(filter.CreatedBy)} has to be a non-zero positive integer (User ID)!");
            }

            photoQuery = photoQuery
                .Where(photo => photo.CreatedBy == filter.CreatedBy);
        }

        // Pagination
        if (filter.Offset is not null)
        {
            if (filter.Offset < 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Offset, $"Pagination Parameter {nameof(filter.Offset)} has to be a positive integer!");
            }

            photoQuery = photoQuery.Skip(filter.Offset.Value);
        }
        if (filter.Limit is not null)
        {
            if (filter.Limit <= 0) {
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
    public Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(filtering);
    }

    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional 
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(FilterPhotosOptions filter)
    {
        IQueryable<PhotoEntity> photoQuery = db.Photos
            .OrderByDescending(photo => photo.CreatedAt)
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Tags);

        // Filtering
        if (filter.Dimension is not null) {
            photoQuery = photoQuery
                .Where(photo => photo.Filepaths.Any(path => path.Dimension == filter.Dimension));
        }

        if (!string.IsNullOrWhiteSpace(filter.Slug))
        {
            photoQuery = photoQuery
                .Where(photo => photo.Slug.StartsWith(filter.Slug) || photo.Slug.EndsWith(filter.Slug));
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            photoQuery = photoQuery
                .Where(photo => !string.IsNullOrWhiteSpace(photo.Title))
                .Where(photo => photo.Title!.StartsWith(filter.Title) || photo.Title.EndsWith(filter.Title));
        }

        if (filter.CreatedAt is not null)
        {
            if (filter.CreatedAt > DateTime.UtcNow) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedAt, $"Filter Parameter {nameof(filter.CreatedAt)} cannot exceed DateTime.UtcNow");
            }
            
            photoQuery = photoQuery
                .Where(photo => photo.CreatedAt >= filter.CreatedAt);
        }

        if (filter.CreatedBy is not null)
        {
            if (filter.CreatedBy <= 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedBy, $"Filter Parameter {nameof(filter.CreatedBy)} has to be a non-zero positive integer (User ID)!");
            }

            photoQuery = photoQuery
                .Where(photo => photo.CreatedBy == filter.CreatedBy);
        }

        // Pagination
        if (filter.Offset is not null)
        {
            if (filter.Offset < 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Offset, $"Pagination Parameter {nameof(filter.Offset)} has to be a positive integer!");
            }

            photoQuery = photoQuery.Skip(filter.Offset.Value);
        }
        if (filter.Limit is not null)
        {
            if (filter.Limit <= 0) {
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


    #region Create / Store a photo blob.
    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="FilterPhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public Task<ActionResult<PhotoCollection>> CreatePhoto(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return CreatePhoto(filtering);
    }

    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="FilterPhotosOptions"/> (<paramref name="details"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public async Task<ActionResult<PhotoCollection>> CreatePhoto(FilterPhotosOptions details)
    {
        string message;
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            message = $"{nameof(CreatePhoto)} Failed: No {nameof(HttpContext)} found.";
            await logging
                .Action(nameof(CreatePhoto))
                .InternalError(message)
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (!MultipartHelper.IsMultipartContentType(httpContext.Request.ContentType))
        {
            message = $"{nameof(CreatePhoto)} Failed: Request couldn't be processed, not a Multipart Formdata request.";
            await logging
                .Action(nameof(CreatePhoto))
                .ExternalError(message)
                .SaveAsync();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }


        var boundary = MultipartHelper.GetBoundary(MediaTypeHeaderValue.Parse(httpContext.Request.ContentType!), 70);
        var reader = new MultipartReader(boundary, httpContext.Request.Body);
        var section = await reader.ReadNextSectionAsync();

    }
    #endregion


    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public Task<ActionResult<PhotoEntity>> CreatePhotoEntity(MutatePhoto mut) =>
        CreatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> CreatePhotoEntity(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(int photoId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(MutatePhoto mut) =>
        UpdatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }
    #endregion


    #region Delete a photo completely (blob, filepaths & photo)
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> DeletePhoto(int photoId)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> DeletePhoto(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }
    #endregion


    #region Delete a blob from disk
    /// <summary>
    /// Deletes the blob of a <see cref="Reception.Models.Entities.Photo"/> from disk.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> DeletePhotoBlob(Filepath entity)
    {
        throw new NotImplementedException();
    }
    #endregion


    #region Delete a photo entities from the database
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public Task<ActionResult<PhotoEntity>> DeletePhotoEntity(MutatePhoto mut) =>
        UpdatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public async Task<ActionResult<PhotoEntity>> DeletePhotoEntity(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }
    #endregion
}