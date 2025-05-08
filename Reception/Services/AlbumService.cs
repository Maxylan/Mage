using Reception.Models;
using Reception.Database.Models;
using Reception.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Reception.Authentication;

namespace Reception.Services;

public class AlbumService(
    MageDbContext db,
    ILoggingService<AlbumService> logging,
    IHttpContextAccessor contextAccessor,
    ITagService tagService
) : IAlbumService
{
    /// <summary>
    /// Get the <see cref="Album"/> with Primary Key '<paramref ref="albumId"/>'
    /// </summary>
    public async Task<ActionResult<Album>> GetAlbum(int albumId)
    {
        if (albumId <= 0)
        {
            throw new ArgumentException($"Parameter {nameof(albumId)} has to be a non-zero positive integer!", nameof(albumId));
        }

        Album? album = await db.Albums.FindAsync(albumId);

        if (album is null)
        {
            string message = $"Failed to find a {nameof(Album)} matching the given {nameof(albumId)} #{albumId}.";
            logging
                .Action(nameof(GetAlbum))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        // Load missing navigation entries.
        foreach (var navigation in db.Entry(album).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        if (album.Thumbnail is not null && album.Thumbnail.Filepaths?.Any() != true)
        {
            album.Thumbnail.Filepaths = await db.Filepaths
                .Where(filepath => filepath.PhotoId == album.ThumbnailId)
                .ToListAsync();
        }

        return album;
    }

    /// <summary>
    /// Get all <see cref="Album"/> instances matching a range of optional filtering / pagination options (<seealso cref="FilterAlbumsOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Album>>> GetAlbums(Action<FilterAlbumsOptions> opts)
    {
        FilterAlbumsOptions filtering = new();
        opts(filtering);

        return GetAlbums(filtering);
    }

    /// <summary>
    /// Get all <see cref="Album"/> instances matching a range of optional filtering / pagination options (<seealso cref="FilterAlbumsOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<Album>>> GetAlbums(FilterAlbumsOptions filter)
    {
        IQueryable<Album> albumQuery = db.Albums
            .OrderByDescending(album => album.CreatedAt)
            .Include(album => album.Thumbnail)
            .Include(album => album.Photos)
            .Include(album => album.AlbumTags);

        // Filtering
        if (filter.MatchPhotoTitles == true)
        {
            throw new NotImplementedException($"{nameof(FilterAlbumsOptions.MatchPhotoTitles)} is a planned feature, maybe..");
        }
        if (filter.MatchPhotoSummaries == true)
        {
            throw new NotImplementedException($"{nameof(FilterAlbumsOptions.MatchPhotoSummaries)} is a planned feature, maybe..");
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            if (!filter.Title.IsNormalized())
            {
                filter.Title = filter.Title
                    .Normalize()
                    .Trim();
            }

            albumQuery = albumQuery
                .Where(album => !string.IsNullOrWhiteSpace(album.Title))
                .Where(album => album.Title!.StartsWith(filter.Title) || album.Title.EndsWith(filter.Title));
        }

        if (!string.IsNullOrWhiteSpace(filter.Summary))
        {
            if (!filter.Summary.IsNormalized())
            {
                filter.Summary = filter.Summary
                    .Normalize()
                    .Trim();
            }

            albumQuery = albumQuery
                .Where(album => !string.IsNullOrWhiteSpace(album.Summary))
                .Where(album => album.Summary!.StartsWith(filter.Summary) || album.Summary.EndsWith(filter.Summary));
        }

        if (filter.CreatedBy is not null)
        {
            if (filter.CreatedBy <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedBy, $"Filter Parameter {nameof(filter.CreatedBy)} has to be a non-zero positive integer (User ID)!");
            }

            albumQuery = albumQuery
                .Where(album => album.CreatedBy == filter.CreatedBy);
        }

        if (filter.CreatedBefore is not null)
        {
            albumQuery = albumQuery
                .Where(album => album.CreatedAt <= filter.CreatedBefore);
        }
        else if (filter.CreatedAfter is not null)
        {
            if (filter.CreatedAfter > DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedAfter, $"Filter Parameter {nameof(filter.CreatedAfter)} cannot exceed DateTime.UtcNow");
            }

            albumQuery = albumQuery
                .Where(album => album.CreatedAt >= filter.CreatedAfter);
        }

        if (filter.Tags is not null && filter.Tags.Length > 0)
        {
            var sanitizeAndCreateTags = await tagService.CreateTags(filter.Tags);
            Tag[]? validTags = sanitizeAndCreateTags.Value?.ToArray();

            if (validTags?.Any() == true)
            {
                albumQuery = albumQuery
                    .Where( // I really hope this makes a valid query..
                        album => album.AlbumTags.Any(tag => validTags.Contains(tag))
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

            albumQuery = albumQuery.Skip(filter.Offset.Value);
        }
        if (filter.Limit is not null)
        {
            if (filter.Limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Limit, $"Pagination Parameter {nameof(filter.Limit)} has to be a non-zero positive integer!");
            }

            albumQuery = albumQuery.Take(filter.Limit.Value);
        }

        return await albumQuery
            .ToListAsync();
    }

    /// <summary>
    /// Get the <see cref="Album"/> with PK <paramref ref="albumId"/> (int), along with a collection of all associated Photos.
    /// </summary>
    public async Task<ActionResult<AlbumPhotoCollection>> GetAlbumPhotoCollection(int albumId)
    {
        var getAlbum = await GetAlbum(albumId);
        Album? album = getAlbum.Value;

        if (album is null) {
            return getAlbum.Result!;
        }

        return new AlbumPhotoCollection(album);
    }

    /// <summary>
    /// Create a new <see cref="Reception.Models.Entities.Album"/>.
    /// </summary>
    public async Task<ActionResult<Album>> CreateAlbum(MutateAlbum mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));

        if (mut.Photos?.Length > 9999)
        {
            mut.Photos = mut.Photos
                .Take(9999)
                .ToArray();
        }
        if (mut.Tags?.Length > 9999)
        {
            mut.Tags = mut.Tags
                .Take(9999)
                .ToArray();
        }

        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(CreateAlbum)} Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(CreateAlbum))
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
                    .Action(nameof(CreateAlbum))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .LogAndEnqueue();
            }
        }

        if (string.IsNullOrWhiteSpace(mut.Title))
        {
            string message = $"Parameter '{nameof(mut.Title)}' may not be null/empty!";
            logging
                .Action(nameof(CreateAlbum))
                .InternalDebug(message, opts => {
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
            string message = $"{nameof(Album.Title)} exceeds maximum allowed length of 255.";
            logging
                .Action(nameof(CreateAlbum))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        bool titleTaken = await db.Albums.AnyAsync(album => album.Title == mut.Title);
        if (titleTaken)
        {
            string message = $"{nameof(Album.Title)} was already taken!";
            logging
                .Action(nameof(CreateAlbum))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(message) {
                StatusCode = StatusCodes.Status409Conflict
            };
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
                string message = $"{nameof(Album.Summary)} exceeds maximum allowed length of 255.";
                logging
                    .Action(nameof(CreateAlbum))
                    .InternalDebug(message, opts => {
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

        PhotoEntity? thumbnail = null;
        if (mut.ThumbnailId is not null && mut.ThumbnailId > 0)
        {
            thumbnail = await db.Photos.FindAsync(mut.ThumbnailId);

            if (thumbnail is null)
            {
                string message = $"{nameof(PhotoEntity)} with ID #{mut.ThumbnailId} could not be found!";
                logging
                    .Action(nameof(CreateAlbum))
                    .InternalDebug(message, opts => {
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                return new NotFoundObjectResult(message);
            }
        }

        Category? category = null;
        if (mut.CategoryId is not null && mut.CategoryId > 0)
        {
            category = await db.Categories.FindAsync(mut.CategoryId);

            if (category is null)
            {
                string message = $"{nameof(Category)} with Title '{mut.Category}' could not be found!";
                logging
                    .Action(nameof(CreateAlbum))
                    .InternalDebug(message, opts => {
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                return new NotFoundObjectResult(message);
            }
        }

        List<Tag>? validTags = null;
        if (mut.Tags?.Any() == true)
        {
            var sanitizeAndCreateTags = await tagService.CreateTags(mut.Tags);
            validTags = sanitizeAndCreateTags.Value?.ToList();
        }

        List<PhotoEntity>? validPhotos = null;
        if (mut.Photos?.Any() == true)
        {
            mut.Photos = mut.Photos
                .Where(photoId => photoId > 0)
                .ToArray();

            validPhotos = await db.Photos
                .Where(photo => mut.Photos.Contains(photo.Id))
                .ToListAsync();
        }

        Album newAlbum = new()
        {
            CategoryId = category?.Id,
            ThumbnailId = thumbnail?.Id,
            Title = mut.Title,
            Summary = mut.Summary,
            Description = mut.Description,
            CreatedBy = user?.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AlbumTags = validTags ?? [],
            Photos = validPhotos ?? [],
            Thumbnail = thumbnail,
            Category = category
        };


        try
        {
            db.Add(newAlbum);

            logging
                .Action(nameof(CreateAlbum))
                .InternalInformation($"A new {nameof(Album)} named '{newAlbum.Title}' was created.", opts =>
                {
                    opts.SetUser(user);
                });

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to create new Album '{newAlbum.Title}'. ";
            logging
                .Action(nameof(CreateAlbum))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to create new Album '{newAlbum.Title}'. ";
            logging
                .Action(nameof(CreateAlbum))
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

        return newAlbum;
    }

    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Album"/> in the database.
    /// </summary>
    public async Task<ActionResult<Album>> UpdateAlbum(MutateAlbum mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));
        ArgumentNullException.ThrowIfNull(mut.Id, nameof(mut.Id));

        if (mut.Photos?.Length > 9999)
        {
            mut.Photos = mut.Photos
                .Take(9999)
                .ToArray();
        }
        if (mut.Tags?.Length > 9999)
        {
            mut.Tags = mut.Tags
                .Take(9999)
                .ToArray();
        }

        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(UpdateAlbum)} Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(UpdateAlbum))
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
                    .Action(nameof(UpdateAlbum))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .LogAndEnqueue();
            }
        }

        if (mut.Id <= 0)
        {
            string message = $"Parameter '{nameof(mut.Id)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(UpdateAlbum))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums.FindAsync(mut.Id);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{mut.Id} could not be found!";
            logging
                .Action(nameof(UpdateAlbum))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        foreach(var navigation in db.Entry(existingAlbum).Navigations)
        {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

        if (string.IsNullOrWhiteSpace(mut.Title))
        {
            string message = $"Parameter '{nameof(mut.Title)}' may not be null/empty!";
            logging
                .Action(nameof(UpdateAlbum))
                .InternalDebug(message, opts => {
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
            string message = $"{nameof(Album.Title)} exceeds maximum allowed length of 255.";
            logging
                .Action(nameof(UpdateAlbum))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (mut.Title != existingAlbum.Title)
        {
            bool titleTaken = await db.Albums.AnyAsync(album => album.Title == mut.Title);
            if (titleTaken)
            {
                string message = $"{nameof(Album.Title)} was already taken!";
                logging
                    .Action(nameof(UpdateAlbum))
                    .InternalDebug(message, opts => {
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                return new ObjectResult(message) {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }
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
                string message = $"{nameof(Album.Summary)} exceeds maximum allowed length of 255.";
                logging
                    .Action(nameof(UpdateAlbum))
                    .InternalDebug(message, opts => {
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

        PhotoEntity? thumbnail = null;
        if (mut.ThumbnailId is not null && mut.ThumbnailId > 0)
        {
            thumbnail = await db.Photos.FindAsync(mut.ThumbnailId);

            if (thumbnail is null)
            {
                string message = $"{nameof(PhotoEntity)} with ID #{mut.ThumbnailId} could not be found!";
                logging
                    .Action(nameof(UpdateAlbum))
                    .InternalDebug(message, opts => {
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                return new NotFoundObjectResult(message);
            }
        }

        Category? category = null;
        if (mut.CategoryId is not null && mut.CategoryId > 0)
        {
            category = await db.Categories.FindAsync(mut.CategoryId);

            if (category is null)
            {
                string message = $"{nameof(Category)} with Title '{mut.Category}' could not be found!";
                logging
                    .Action(nameof(UpdateAlbum))
                    .InternalDebug(message, opts => {
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                return new NotFoundObjectResult(message);
            }
        }

        List<Tag>? validTags = null;
        if (mut.Tags?.Any() == true)
        {
            var sanitizeAndCreateTags = await tagService.CreateTags(mut.Tags);
            validTags = sanitizeAndCreateTags.Value?.ToList();
        }

        List<PhotoEntity>? validPhotos = null;
        if (mut.Photos?.Any() == true)
        {
            mut.Photos = mut.Photos
                .Where(photoId => photoId > 0)
                .ToArray();

            validPhotos = await db.Photos
                .Where(photo => mut.Photos.Contains(photo.Id))
                .ToListAsync();
        }

        existingAlbum.Title = mut.Title;
        existingAlbum.Summary = mut.Summary;
        existingAlbum.Description = mut.Description;
        existingAlbum.UpdatedAt = DateTime.UtcNow;

        if (mut.Tags is not null) {
            existingAlbum.AlbumTags = validTags ?? [];
        }

        if (mut.Photos is not null) {
            existingAlbum.Photos = validPhotos ?? [];
        }

        if (mut.ThumbnailId is not null && mut.ThumbnailId > 0) {
            existingAlbum.Thumbnail = thumbnail;
            existingAlbum.ThumbnailId = thumbnail?.Id;
        }

        if (mut.CategoryId is not null && mut.CategoryId > 0) {
            existingAlbum.Category = category;
            existingAlbum.CategoryId = category?.Id;
        }

        try
        {
            db.Update(existingAlbum);

            if (Program.IsDevelopment)
            {
                logging
                    .Action(nameof(UpdateAlbum))
                    .InternalDebug($"An {nameof(Album)} ('{existingAlbum.Title}', #{existingAlbum.Id}) was just updated.", opts =>
                    {
                        opts.SetUser(user);
                    });
            }

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(UpdateAlbum))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(UpdateAlbum))
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

        return existingAlbum;
    }

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (int[]).
    /// </summary>
    public async Task<ActionResult<AlbumPhotoCollection>> MutateAlbumPhotos(int albumId, int[] photoIds)
    {
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

        if (photoIds.Length > 9999)
        {
            photoIds = photoIds
                .Take(9999)
                .ToArray();
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(MutateAlbumPhotos))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums.FindAsync(albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
            logging
                .Action(nameof(MutateAlbumPhotos))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        foreach(var navigation in db.Entry(existingAlbum).Navigations)
        {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

        photoIds = photoIds
            .Where(photoId => photoId > 0)
            .ToArray();

        var existingIds = existingAlbum.Photos
            .Select(photo => photo.Id)
            .ToArray();

        if (photoIds.Length <= 0 || photoIds == existingIds) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        existingAlbum.Photos = await db.Photos
            .Where(photo => photoIds.Contains(photo.Id))
            .ToListAsync();

        try
        {
            db.Update(existingAlbum);

            if (Program.IsDevelopment)
            {
                logging
                    .Action(nameof(MutateAlbumPhotos))
                    .InternalDebug($"The photos in an {nameof(Album)} ('{existingAlbum.Title}', #{existingAlbum.Id}) was just updated.");
            }

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update the photos of an existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(MutateAlbumPhotos))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update the photos of the existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(MutateAlbumPhotos))
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

        return new AlbumPhotoCollection(existingAlbum);
    }

    /// <summary>
    /// Removes a <see cref="Reception.Models.Entities.PhotoEntity"/> (..identified by PK <paramref name="photoId"/>) from the
    /// <see cref="Reception.Models.Entities.Album"/> identified by its PK <paramref name="albumId"/>.
    /// </summary>
    public async Task<ActionResult> RemovePhoto(int albumId, int photoId)
    {
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));
        ArgumentNullException.ThrowIfNull(photoId, nameof(photoId));

        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(RemovePhoto))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(RemovePhoto))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums
            .Include(album => album.Photos)
            .FirstOrDefaultAsync(album => album.Id == albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
            logging
                .Action(nameof(RemovePhoto))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        PhotoEntity? photoToRemove = existingAlbum.Photos
            .FirstOrDefault(photo => photo.Id == photoId);

        if (photoToRemove is null) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        existingAlbum.Photos.Remove(photoToRemove);

        try
        {
            db.Update(existingAlbum);

            logging
                .Action(nameof(RemovePhoto))
                .InternalTrace($"A photo was just removed from {nameof(Album)} ('{existingAlbum.Title}', #{existingAlbum.Id})");

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to remove a photo from Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(RemovePhoto))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to remove a photo from Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(RemovePhoto))
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

    /// <summary>
    /// Removes a <see cref="Reception.Models.Entities.Tag"/> (..identified by PK <paramref name="tagId"/>) from the
    /// <see cref="Reception.Models.Entities.Album"/> identified by its PK <paramref name="albumId"/>.
    /// </summary>
    public async Task<ActionResult> RemoveTag(int albumId, string tag)
    {
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

        if (string.IsNullOrWhiteSpace(tag))
        {
            string message = $"Parameter '{nameof(tag)}' was null/empty!";
            logging
                .Action(nameof(RemoveTag))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(RemoveTag))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums
            .Include(album => album.AlbumTags)
            .FirstOrDefaultAsync(album => album.Id == albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
            logging
                .Action(nameof(RemoveTag))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        Tag? tagToRemove = existingAlbum.AlbumTags
            .FirstOrDefault(t => t.Name == tag);

        if (tagToRemove is null) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        existingAlbum.AlbumTags.Remove(tagToRemove);

        try
        {
            db.Update(existingAlbum);

            logging
                .Action(nameof(RemoveTag))
                .InternalTrace($"A tag was just removed from {nameof(Album)} ('{existingAlbum.Title}', #{existingAlbum.Id})");

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to remove a tag from Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(RemoveTag))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to remove a tag from Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(RemoveTag))
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

    /// <summary>
    /// Deletes the <see cref="Reception.Models.Entities.Album"/> identified by <paramref name="albumId"/>
    /// </summary>
    public async Task<ActionResult> DeleteAlbum(int albumId)
    {
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(DeleteAlbum))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums.FindAsync(albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
            logging
                .Action(nameof(DeleteAlbum))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        try
        {
            db.Remove(existingAlbum);

            logging
                .Action(nameof(DeleteAlbum))
                .InternalWarning($"The {nameof(Album)} ('{existingAlbum.Title}', #{existingAlbum.Id}) was just deleted.");

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(Album)} '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(DeleteAlbum))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(Album)} '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(DeleteAlbum))
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
}
