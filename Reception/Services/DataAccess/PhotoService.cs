using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Middleware.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Utilities;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;
using System.Net;

namespace Reception.Services.DataAccess;

public class PhotoService(
    MageDb db,
    ILoggingService<PhotoService> logging,
    IHttpContextAccessor contextAccessor,
    ITagService tagService
) : IPhotoService
{
    #region Get single photos.
    /// <summary>
    /// Get the <see cref="Photo"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<Photo>> GetPhoto(int photoId)
    {
        if (photoId <= 0)
        {
            throw new ArgumentException($"Parameter {nameof(photoId)} has to be a non-zero positive integer!", nameof(photoId));
        }

        Photo? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(Photo)} matching the given {nameof(photoId)} #{photoId}.";
            logging
                .Action(nameof(GetPhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

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
                .Action(nameof(GetPhoto))
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
                .Action(nameof(GetPhoto))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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
    /// Get the <see cref="Photo"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<Photo>> GetPhoto(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Photo? photo = await db.Photos
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.PublicLinks)
            .Include(photo => photo.Tags)
            .FirstOrDefaultAsync(photo => photo.Slug == slug);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(Photo)} matching the given {nameof(slug)} '{slug}'.";
            logging
                .Action(nameof(GetPhoto))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

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
                .Action(nameof(GetPhoto))
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
                .Action(nameof(GetPhoto))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return photo;
    }
    #endregion


    #region Get many photos.
    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Photo>>> GetPhotos(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(filtering);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{Reception.Database.Models.Photo}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos(FilterPhotosOptions filter)
    {
        filter.Dimension ??= Dimension.SOURCE;

        IQueryable<Photo> photoQuery = db.Photos
            .OrderByDescending(photo => photo.CreatedAt)
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.PublicLinks)
            .Include(photo => photo.Tags)
            .Where(photo => photo.Filepaths.Any(path => path.Dimension == filter.Dimension));

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
                .Action(nameof(GetPhotos))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        // Filter by privilege
        photoQuery = photoQuery
            .Where(photo => (user.Privilege & (photo.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL))) == (photo.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL)));

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
                        photo => photo.Tags.Any(relation => validTags.Contains(relation.Tag))
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

        return await photoQuery.ToListAsync();
    }


    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances by evaluating a wide range of optional search / pagination options (<seealso cref="PhotoSearchQuery"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Photo>>> PhotoSearch(Action<PhotoSearchQuery> opts)
    {
        PhotoSearchQuery search = new();
        opts(search);

        return PhotoSearch(search);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{Reception.Database.Models.Photo}"/> collection of Photos by evaluating a wide range of optional
    /// search / pagination options (<seealso cref="PhotoSearchQuery"/>).
    /// </summary>
    public Task<ActionResult<IEnumerable<Photo>>> PhotoSearch(PhotoSearchQuery searchQuery)
    {
        throw new NotImplementedException();
        // Searching (OR)
        /* if (!string.IsNullOrWhiteSpace(search))
        {
            photoQuery = photoQuery
                .Where(photo => (
                    photo.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || photo.Slug.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || photo.Summary.Contains(search, StringComparison.OrdinalIgnoreCase)
                ));
        } */
    }
    #endregion


    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<Photo>> CreatePhoto(MutatePhoto mut)
    {
        Photo entity = (Photo)mut;

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
                .Action(nameof(CreatePhoto))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.CREATE | mut.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(CreatePhoto))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if (mut.Tags is not null)
        {
            if (mut.Tags.Count() > 9999)
            {
                mut.Tags = mut.Tags
                    .Take(9999)
                    .ToArray();
            }

            if (mut.Tags.Count() > 0)
            {
                var sanitizeAndCreateTags = await tagService.CreateTags(mut.Tags);

                if (sanitizeAndCreateTags.Value is not null) {
                    foreach(Tag tag in sanitizeAndCreateTags.Value) {
                        entity.Tags.Add(new() {
                            Tag = tag,
                            Added = DateTime.Now
                        });
                    }
                }
            }
        }

        if (mut.Albums is not null)
        {
            if (mut.Albums.Count() > 9999)
            {
                mut.Albums = mut.Albums
                    .Take(9999)
                    .ToArray();
            }

            if (mut.Albums.Count() > 0)
            {
                foreach(int albumId in mut.Albums) {
                    entity.Albums.Add(new() {
                        AlbumId = albumId,
                        Added = DateTime.Now
                    });
                }
            }
        }

        return await CreatePhoto(entity);
    }

    /// <summary>
    /// Create a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<Photo>> CreatePhoto(Photo entity)
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
                string message = $"Can't save bad {nameof(Photo)} (#{entity.Id}) to database, entity has no '{Dimension.SOURCE.ToString()}' {nameof(Filepath)} and both '{nameof(Photo.Slug)}' & '{nameof(Photo.Title)}' are null/omitted!";
                logging
                    .Action(nameof(CreatePhoto))
                    .ExternalWarning(message)
                    .LogAndEnqueue();

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
                    string message = $"{nameof(Photo)} with unique '{entity.Slug}' already exists!";
                    logging
                        .Action(nameof(CreatePhoto))
                        .ExternalWarning(message)
                        .LogAndEnqueue();

                    return new ObjectResult(message)
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                }
            }
        }

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
                .Action(nameof(CreatePhoto))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.CREATE | entity.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(CreatePhoto))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        try
        {
            db.Add(entity);
            await db.SaveChangesAsync();

            if (entity.Id == default)
            {
                await db.Entry(entity).ReloadAsync();
            }

            logging
                .Action(nameof(CreatePhoto))
                .InternalTrace($"Created new {nameof(Photo)} '{entity.Slug}' (#{entity.Id})", opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();
        }
        catch (DbUpdateConcurrencyException concurrencyException)
        {
            string message = string.Empty;
            bool exists = await db.Photos.ContainsAsync(entity);

            if (exists)
            {
                message = $"{nameof(Photo)} '{entity.Slug}' (#{entity.Id}) already exists!";
                logging
                    .Action(nameof(CreatePhoto))
                    .InternalError(message, opts =>
                    {
                        opts.Exception = concurrencyException;
                    })
                    .LogAndEnqueue();

                return new ObjectResult(message)
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            message = $"Cought a {nameof(DbUpdateConcurrencyException)} while attempting to save '{entity.Slug}' (#{entity.Id}) to database! ";
            logging
                .Action(nameof(CreatePhoto))
                .InternalError(message + concurrencyException.Message, opts =>
                {
                    opts.Exception = concurrencyException;
                })
                .LogAndEnqueue();

            return new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {updateException.GetType().Name} while attempting to save '{entity.Slug}' (#{entity.Id}) to database! ";
            logging
                .Action(nameof(CreatePhoto))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .LogAndEnqueue();

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
    /// Toggles the 'Favorite' status of a <see cref="Reception.Database.Models.Photo"/> for a single user.
    /// </summary>
    public async Task<ActionResult> ToggleFavorite(int photoId)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(ToggleFavorite)} Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(ToggleFavorite))
                .InternalError(message)
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

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
                .Action(nameof(ToggleFavorite))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if (photoId <= 0)
        {
            string message = $"Parameter {nameof(photoId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(ToggleFavorite))
                .ExternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        Photo? existingPhoto = await db.Photos.FindAsync(photoId);

        if (existingPhoto is null)
        {
            string message = $"{nameof(Photo)} with ID #{photoId} could not be found!";
            logging
                .Action(nameof(ToggleFavorite))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        byte privilegeRequired = (byte)
            (Privilege.UPDATE | existingPhoto.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(ToggleFavorite))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        foreach (var navigation in db.Entry(existingPhoto).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        string actionTaken = string.Empty;
        var relation = existingPhoto.FavoritedBy?.FirstOrDefault(
            relation => relation.AccountId == user.Id
        );
        
        if (relation is null) {
            actionTaken = "Favorited";
            relation = new() {
                AccountId = user.Id,
                Account = user,
                PhotoId = existingPhoto.Id,
                Photo = existingPhoto,
                Added = DateTime.UtcNow
            };

            db.Add(relation);
        }
        else {
            actionTaken = "Un-favorited";
            existingPhoto.FavoritedBy!.Remove(relation);
            db.Remove(relation);
        }

        try
        {
            db.Update(existingPhoto);

            logging
                .Action(nameof(ToggleFavorite))
                .InternalTrace($"User '{user.Username}' {actionTaken} {nameof(Photo)} '{existingPhoto.Slug}' (#{existingPhoto.Id}).", opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update existing {nameof(Photo)} '{existingPhoto.Slug}'. ";
            logging
                .Action(nameof(ToggleFavorite))
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
                .Action(nameof(ToggleFavorite))
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

        return new OkResult();
    }


    /// <summary>
    /// Updates a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<Photo>> UpdatePhoto(MutatePhoto mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));
        ArgumentNullException.ThrowIfNull(mut.Id, nameof(mut.Id));

        if (mut.Tags?.Count() > 9999)
        {
            mut.Tags = mut.Tags
                .Take(9999)
                .ToArray();
        }

        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(UpdatePhoto)} Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(UpdatePhoto))
                .InternalError(message)
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

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
                .Action(nameof(UpdatePhoto))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if (mut.Id <= 0)
        {
            string message = $"Parameter '{nameof(mut.Id)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(UpdatePhoto))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Photo? existingPhoto = await db.Photos.FindAsync(mut.Id);

        if (existingPhoto is null)
        {
            string message = $"{nameof(Photo)} with ID #{mut.Id} could not be found!";
            logging
                .Action(nameof(UpdatePhoto))
                .InternalDebug(message, opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        byte privilegeRequired = (byte)
            (Privilege.UPDATE | mut.RequiredPrivilege | existingPhoto.RequiredPrivilege);

        if ((mut.RequiredPrivilege & existingPhoto.RequiredPrivilege) != existingPhoto.RequiredPrivilege) {
            privilegeRequired = (byte)
                (Privilege.ADMIN | privilegeRequired);
        }

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(UpdatePhoto))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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
                .Action(nameof(UpdatePhoto))
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
            string message = $"{nameof(Photo.Slug)} exceeds maximum allowed length of 127.";
            logging
                .Action(nameof(UpdatePhoto))
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
                string message = $"{nameof(Photo.Slug)} was already taken!";
                logging
                    .Action(nameof(UpdatePhoto))
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
                .Action(nameof(UpdatePhoto))
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
            string message = $"{nameof(Photo.Title)} exceeds maximum allowed length of 255.";
            logging
                .Action(nameof(UpdatePhoto))
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
                string message = $"{nameof(Photo.Summary)} exceeds maximum allowed length of 255.";
                logging
                    .Action(nameof(UpdatePhoto))
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

        List<PhotoTagRelation> tagRelations = [];
        if (mut.Tags?.Any() == true)
        {
            var sanitizeAndCreateTags = await tagService.CreateTags(mut.Tags);

            if (sanitizeAndCreateTags.Value is not null) {
                foreach(Tag tag in sanitizeAndCreateTags.Value) {
                    tagRelations.Add(new() {
                        Tag = tag,
                        Added = DateTime.Now
                    });
                }
            }
        }

        existingPhoto.Slug = mut.Slug;
        existingPhoto.Title = mut.Title;
        existingPhoto.Summary = mut.Summary;
        existingPhoto.Description = mut.Description;
        existingPhoto.UpdatedAt = DateTime.UtcNow;
        if (user is not null) { // TODO - FIX ME
            existingPhoto.UpdatedBy = user.Id;
        }

        if (mut.Tags is not null)
        {
            existingPhoto.Tags = tagRelations;
        }

        try
        {
            db.Update(existingPhoto);

            logging
                .Action(nameof(UpdatePhoto))
                .InternalTrace($"{nameof(Photo)} '{existingPhoto.Slug}' (#{existingPhoto.Id}) was just updated.", opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update existing {nameof(Photo)} '{existingPhoto.Slug}'. ";
            logging
                .Action(nameof(UpdatePhoto))
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
                .Action(nameof(UpdatePhoto))
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
    /// Adds the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) to the
    /// <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Tag>>> AddTags(int photoId, IEnumerable<ITag> tags)
    {
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999)
                .ToArray();
        }

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(AddTags))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Photo? existingPhoto = await db.Photos.FindAsync(photoId);

        if (existingPhoto is null)
        {
            string message = $"{nameof(Photo)} with ID #{photoId} could not be found!";
            logging
                .Action(nameof(AddTags))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        foreach(var navigation in db.Entry(existingPhoto).Navigations)
        {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

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
                .Action(nameof(AddTags))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.UPDATE | existingPhoto.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(AddTags))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        var existingTags = existingPhoto.Tags
            .Select(relation => relation.Tag.Name);

        tags = tags
            .Where(tag => (user.Privilege & (tag.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL))) == (tag.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL)))
            .Distinct()
            .IntersectBy(existingTags, tag => tag.Name)
            .ToList();

        var newTags = await tagService.CreateTags(tags);

        if (newTags.Value is null)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(AddTags))
                .InternalDebug(message)
                .LogAndEnqueue();

            return newTags.Result!;
        }

        if (newTags.Value.Count() <= 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var newTagRelations =
            newTags.Value.Select(tag => new PhotoTagRelation() {
                TagId = tag.Id,
                Tag = tag,
                PhotoId = existingPhoto.Id,
                Photo = existingPhoto,
                Added = DateTime.Now
            })
            .Concat(existingPhoto.Tags)
            .ToList();

        existingPhoto.Tags = newTagRelations;
        try
        {
            db.Update(existingPhoto);

            logging
                .Action(nameof(AddTags))
                .ExternalTrace(
                    $"The tags of {nameof(Photo)} '{existingPhoto.Title}' (#{existingPhoto.Id}) was just updated.",
                    opts => {
                        opts.SetUser(user);
                    }
                )
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to add tags to the existing Photo '{existingPhoto.Title}'. ";
            logging
                .Action(nameof(AddTags))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to add tags to the existing Photo '{existingPhoto.Title}'. ";
            logging
                .Action(nameof(AddTags))
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

        return existingPhoto.Tags
            .Select(relation => relation.Tag)
            .ToArray();
    }

    /// <summary>
    /// Removes the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) from
    /// the <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Tag>>> RemoveTags(int photoId, IEnumerable<ITag> tags)
    {
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999)
                .ToArray();
        }

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(RemoveTags))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Photo? existingPhoto = await db.Photos.FindAsync(photoId);

        if (existingPhoto is null)
        {
            string message = $"{nameof(Photo)} with ID #{photoId} could not be found!";
            logging
                .Action(nameof(RemoveTags))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        foreach(var navigation in db.Entry(existingPhoto).Navigations)
        {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

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
                .Action(nameof(RemoveTags))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.UPDATE | existingPhoto.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(RemoveTags))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        var tagNames = tags
            .Select(tag => tag.Name)
            .Distinct();

        if (tagNames.Count() <= 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var newRelations = existingPhoto.Tags
            .IntersectBy(tagNames, tag => tag.Tag.Name)
            .ToList();

        existingPhoto.Tags = newRelations;
        try
        {
            db.Update(existingPhoto);

            logging
                .Action(nameof(RemoveTags))
                .ExternalTrace(
                    $"The tags in {nameof(Photo)} '{existingPhoto.Title}' (#{existingPhoto.Id}) was just updated.",
                    opts => {
                        opts.SetUser(user);
                    }
                )
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to remove photos from the existing Photo '{existingPhoto.Title}'. ";
            logging
                .Action(nameof(RemoveTags))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to remove photos from the existing Photos '{existingPhoto.Title}'. ";
            logging
                .Action(nameof(RemoveTags))
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

        return existingPhoto.Tags
            .Select(relation => relation.Tag)
            .ToArray();
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

        Photo? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"{nameof(Photo)} with ID #{photoId} could not be found!";
            logging
                .Action(nameof(DeletePhoto))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        return await DeletePhoto(photo);
    }
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..identified by PK <paramref name="entity"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult> DeletePhoto(Photo entity)
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
            if (path.Photo is null) {
                path.Photo = entity;
            }

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
                .Action(nameof(DeletePhotoBlob))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.DELETE | entity.Photo.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(DeletePhotoBlob))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        string fullPath = Path.Combine(entity.Path, entity.Filename);
        if (fullPath.Contains("&&") || fullPath.Contains("..") || fullPath.Length > 511)
        {
            logging
                .Action(nameof(DeletePhotoBlob))
                .ExternalSuspicious($"Sussy filpath '{fullPath}' (TODO! HANDLE)", opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            throw new NotImplementedException("Suspicious?"); // TODO! Handle!!
        }

        if (!File.Exists(fullPath))
        {
            string message = string.Empty;

            if (!fullPath.Contains(Postbox.FILE_STORAGE_NAME))
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
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            if (Program.IsProduction)
            {
                return new NotFoundResult();
            }

            return new NotFoundObjectResult(message);
        }

        try
        {
            await Task.Run(() => File.Delete(fullPath));

            logging
                .Action(nameof(DeletePhotoBlob))
                .InternalTrace($"The blob on path '{fullPath}' (Filepath ID #{entity.Id}) was just deleted.", opts => {
                    opts.SetUser(user);
                })
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
                .Action(nameof(DeletePhotoBlob))
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

        return new NoContentResult();
    }
    #endregion


    #region Delete a photo entities from the database
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..and associated <see cref="Reception.Database.Models.Filepath"/> entities) ..from the database.
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

        Photo? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"{nameof(Photo)} with ID #{photoId} could not be found!";
            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        return await DeletePhotoEntity(photo);
    }

    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..and associated <see cref="Reception.Database.Models.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public async Task<ActionResult> DeletePhotoEntity(Photo entity)
    {
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
                .Action(nameof(DeletePhotoEntity))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.DELETE | entity.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(DeletePhotoEntity))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        try
        {
            db.Remove(entity);

            logging
                .Action(nameof(DeletePhotoEntity))
                .InternalTrace($"The {nameof(Photo)} '{entity.Title}' (#{entity.Slug}, #{entity.Id}) was just deleted.")
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(Photo)} '{entity.Title}'. ";
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(Photo)} '{entity.Title}'. ";
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
