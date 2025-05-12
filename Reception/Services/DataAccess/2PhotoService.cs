using SixLabors.ImageSharp;
using System.Net;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Middleware.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Services.DataAccess;

public class TwoPhotoService(
    MageDb db,
    ILoggingService<PhotoService> logging,
    IHttpContextAccessor contextAccessor,
    ITagService tagService
) : IPhotoService
{
    #region Get single photos.
    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>

    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>


    /// <summary>
    /// Get the <see cref="Reception.Database.Models.Photo"/> with Primary Key '<paramref ref="photoId"/>'
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
            logging
                .Action(nameof(GetSinglePhoto))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new Photo(entity, dimension);
    }

    /// <summary>
    /// Get the <see cref="Reception.Database.Models.Photo"/> with Slug '<paramref ref="slug"/>' (string)
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
            logging
                .Action(nameof(GetSinglePhoto))
                .LogDebug(message)
                .LogAndEnqueue();

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
            logging
                .Action(nameof(GetPhoto))
                .LogDebug(message)
                .LogAndEnqueue();

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

            logging
                .Action(nameof(GetPhoto))
                .InternalWarning(message)
                .LogAndEnqueue();

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
            logging
                .Action(nameof(GetPhoto))
                .LogDebug(message)
                .LogAndEnqueue();

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

            logging
                .Action(nameof(GetPhoto))
                .InternalWarning(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new PhotoCollection(photo);
    }
    #endregion


    #region Get many photos.
    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>

    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>


    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.DisplayPhoto}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(string? search, Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(search, filtering);
    }

    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.DisplayPhoto}"/> collection of Photos matching a wide range of optional
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

    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Updates a <see cref="Reception.Database.Models.PhotoEntity"/> in the database.
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
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalError(message)
                .LogAndEnqueue();

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
                logging
                    .Action(nameof(UpdatePhotoEntity))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .LogAndEnqueue();
            }
        }

        if (mut.Id <= 0)
        {
            string message = $"Parameter '{nameof(mut.Id)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        PhotoEntity? existingPhoto = await db.Photos.FindAsync(mut.Id);

        if (existingPhoto is null)
        {
            string message = $"{nameof(Album)} with ID #{mut.Id} could not be found!";
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

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
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

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
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (mut.Slug != existingPhoto.Slug)
        {
            bool slugTaken = await db.Photos.AnyAsync(photo => photo.Slug == mut.Slug);
            if (slugTaken)
            {
                string message = $"{nameof(PhotoEntity.Slug)} was already taken!";
                logging
                    .Action(nameof(UpdatePhotoEntity))
                    .InternalDebug(message, opts =>
                    {
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                return new ObjectResult(message)
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }
        }

        if (string.IsNullOrWhiteSpace(mut.Title))
        {
            string message = $"Parameter '{nameof(mut.Title)}' may not be null/empty!";
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

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
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

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
                logging
                    .Action(nameof(UpdatePhotoEntity))
                    .InternalDebug(message, opts =>
                    {
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

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
                })
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update existing Album '{existingPhoto.Slug}'. ";
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

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
            logging
                .Action(nameof(UpdatePhotoEntity))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

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
    /// Removes a <see cref="Reception.Database.Models.Tag"/> (..identified by <paramref name="tag"/>) from the
    /// <see cref="Reception.Database.Models.PhotoEntity"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult> RemoveTag(int photoId, string tag)
    {
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (string.IsNullOrWhiteSpace(tag))
        {
            string message = $"Parameter '{nameof(tag)}' was null/empty!";
            logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        PhotoEntity? existingPhoto = await db.Photos
            .Include(photo => photo.Tags)
            .FirstOrDefaultAsync(photo => photo.Id == photoId);

        if (existingPhoto is null)
        {
            string message = $"{nameof(PhotoEntity)} with ID #{photoId} could not be found!";
            logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalDebug(message)
                .LogAndEnqueue();

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
                .InternalTrace($"A tag was just removed from {nameof(PhotoEntity)} ('{existingPhoto.Title}', #{existingPhoto.Id})")
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to remove a tag from Photo '{existingPhoto.Title}'. ";
            logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .LogAndEnqueue();

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
            logging
                .Action(nameof(RemoveTag) + $" ({nameof(PhotoService)})")
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

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
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(DeletePhoto))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"{nameof(PhotoEntity)} with ID #{photoId} could not be found!";
            logging
                .Action(nameof(DeletePhoto))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        return await DeletePhoto(photo);
    }
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
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
    /// Deletes the blob of a <see cref="Reception.Database.Models.Photo"/> from disk.
    /// </summary>
    public async Task<ActionResult> DeletePhotoBlob(Filepath entity)
    {
        string fullPath = Path.Combine(entity.Path, entity.Filename);
        if (fullPath.Contains("&&") || fullPath.Contains("..") || fullPath.Length > 511)
        {
            logging
                .Action(nameof(DeletePhotoBlob))
                .ExternalSuspicious($"Sussy filpath '{fullPath}' (TODO! HANDLE)")
                .LogAndEnqueue();

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

            logging
                .ExternalSuspicious(message)
                .LogAndEnqueue();

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
                .InternalInformation($"The blob on path '{fullPath}' (Filepath ID #{entity.Id}) was just deleted.")
                .LogAndEnqueue();
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
            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

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
    /// Deletes a <see cref="Reception.Database.Models.PhotoEntity"/> (..and associated <see cref="Reception.Database.Models.Filepath"/> entities) ..from the database.
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
            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"{nameof(PhotoEntity)} with ID #{photoId} could not be found!";
            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        return await DeletePhotoEntity(photo);
    }

    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.PhotoEntity"/> (..and associated <see cref="Reception.Database.Models.Filepath"/> entities) ..from the database.
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
                .InternalInformation($"The {nameof(PhotoEntity)} ('{entity.Title}', #{entity.Id}) was just deleted.")
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(PhotoEntity)} '{entity.Title}'. ";
            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .LogAndEnqueue();

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
            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

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
