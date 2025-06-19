using System.Net;
using Reception.Models;
using Reception.Database;
using Reception.Database.Models;
using Reception.Interfaces;
using Reception.Interfaces.DataAccess;
using Reception.Middleware.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Services.DataAccess;

public class AlbumService(
    MageDb db,
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
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(GetAlbum))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
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
                .Action(nameof(GetAlbum))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredViewPrivilege = (byte)
            (album.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL));

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(GetAlbum))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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
            .Include(album => album.Tags);

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
                .Action(nameof(GetAlbums))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        // Filter by privilege
        albumQuery = albumQuery
            .Where(album => (user.Privilege & (album.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL))) == (album.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL)));

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
                    .Where( // I really hope nested-any's still makes a valid query..
                        album => album.Tags.Any(at => validTags.Any(tag => tag.Id == at.TagId))
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
    /// Create a new <see cref="Reception.Database.Models.Album"/>.
    /// </summary>
    public async Task<ActionResult<Album>> CreateAlbum(MutateAlbum mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));

        if (mut.Photos?.Count() > 9999)
        {
            mut.Photos = mut.Photos
                .Take(9999);
        }
        if (mut.Tags?.Count() > 9999)
        {
            mut.Tags = mut.Tags
                .Take(9999);
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
                .Action(nameof(CreateAlbum))
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
                .Action(nameof(CreateAlbum))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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

        Photo? thumbnail = null;
        if (mut.ThumbnailId is not null && mut.ThumbnailId > 0)
        {
            thumbnail = await db.Photos.FindAsync(mut.ThumbnailId);

            if (thumbnail is null)
            {
                string message = $"{nameof(Photo)} with ID #{mut.ThumbnailId} could not be found!";
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

        List<AlbumTagRelation> tagRelations = [];
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

        List<PhotoAlbumRelation> photoRelations = [];
        if (mut.Photos?.Any() == true)
        {
            mut.Photos = mut.Photos
                .Where(photoId => photoId > 0)
                .ToArray();

            var validPhotos = await db.Photos
                .Where(photo => mut.Photos.Contains(photo.Id))
                .ToListAsync();

            foreach(Photo photo in validPhotos) {
                photoRelations.Add(new() {
                    Photo = photo,
                    Added = DateTime.Now
                });
            }
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
            Tags = tagRelations,
            Photos = photoRelations,
            Thumbnail = thumbnail,
            Category = category
        };


        try
        {
            db.Add(newAlbum);

            logging
                .Action(nameof(CreateAlbum))
                .ExternalTrace($"A new {nameof(Album)} named '{newAlbum.Title}' was created.", opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

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
    /// Updates a <see cref="Reception.Database.Models.Album"/> in the database.
    /// </summary>
    public async Task<ActionResult<Album>> UpdateAlbum(MutateAlbum mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));
        ArgumentNullException.ThrowIfNull(mut.Id, nameof(mut.Id));

        if (mut.Photos?.Count() > 9999)
        {
            mut.Photos = mut.Photos
                .Take(9999);
        }
        if (mut.Tags?.Count() > 9999)
        {
            mut.Tags = mut.Tags
                .Take(9999);
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
                .Action(nameof(UpdateAlbum))
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

        byte privilegeRequired = (byte)
            (Privilege.UPDATE | mut.RequiredPrivilege | existingAlbum.RequiredPrivilege);

        if ((mut.RequiredPrivilege & existingAlbum.RequiredPrivilege) != existingAlbum.RequiredPrivilege) {
            privilegeRequired = (byte)
                (Privilege.ADMIN | privilegeRequired);
        }

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(UpdateAlbum))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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

        Photo? thumbnail = null;
        if (mut.ThumbnailId is not null && mut.ThumbnailId > 0)
        {
            thumbnail = await db.Photos.FindAsync(mut.ThumbnailId);

            if (thumbnail is null)
            {
                string message = $"{nameof(Photo)} with ID #{mut.ThumbnailId} could not be found!";
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

        List<AlbumTagRelation> tagRelations = [];
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

        List<PhotoAlbumRelation> photoRelations = [];
        if (mut.Photos?.Any() == true)
        {
            mut.Photos = mut.Photos
                .Where(photoId => photoId > 0)
                .ToArray();

            var validPhotos = await db.Photos
                .Where(photo => mut.Photos.Contains(photo.Id))
                .ToListAsync();

            foreach(Photo photo in validPhotos) {
                photoRelations.Add(new() {
                    Photo = photo,
                    Added = DateTime.Now
                });
            }
        }

        existingAlbum.Title = mut.Title;
        existingAlbum.Summary = mut.Summary;
        existingAlbum.Description = mut.Description;
        existingAlbum.UpdatedAt = DateTime.UtcNow;
        existingAlbum.UpdatedBy = user.Id;

        if (mut.Tags is not null) {
            existingAlbum.Tags = tagRelations;
        }

        if (mut.Photos is not null) {
            existingAlbum.Photos = photoRelations;
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

            logging
                .Action(nameof(UpdateAlbum))
                .ExternalTrace($"{nameof(Album)} '{existingAlbum.Title}' (#{existingAlbum.Id}) was just updated.", opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

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
    /// Add <see cref="Tag"/>(s) (<paramref name="tags"/>) ..to a <see cref="Album"/> identified by PK '<paramref ref="albumId"/>' (int)
    /// </summary>
    public async Task<ActionResult> ToggleFavorite(int albumId)
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

        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
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

        Album? existingAlbum = await db.Albums.FindAsync(albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
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
            (Privilege.UPDATE | existingAlbum.RequiredPrivilege);

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

        foreach (var navigation in db.Entry(existingAlbum).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        string actionTaken = string.Empty;
        var relation = existingAlbum.FavoritedBy?.FirstOrDefault(
            relation => relation.AccountId == user.Id
        );
        
        if (relation is null) {
            actionTaken = "Favorited";
            relation = new() {
                AccountId = user.Id,
                Account = user,
                AlbumId = existingAlbum.Id,
                Album = existingAlbum,
                Added = DateTime.UtcNow
            };

            db.Add(relation);
        }
        else {
            actionTaken = "Un-favorited";
            existingAlbum.FavoritedBy!.Remove(relation);
            db.Remove(relation);
        }

        try
        {
            db.Update(existingAlbum);

            logging
                .Action(nameof(ToggleFavorite))
                .InternalTrace(
                    $"User '{user.Username}' {actionTaken} {nameof(Album)} '{existingAlbum.Title}' (#{existingAlbum.Id}).",
                    opts => { opts.SetUser(user); }
                )
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update existing {nameof(Album)} '{existingAlbum.Title}'. ";
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update existing Album '{existingAlbum.Title}'. ";
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
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (<see cref="IEnumerable{int}"/>).
    /// </summary>
    public async Task<ActionResult<Album>> AddPhotos(int albumId, IEnumerable<int> photoIds)
    {
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

        if (photoIds.Count() > 9999)
        {
            photoIds = photoIds
                .Take(9999)
                .ToArray();
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(AddPhotos))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums.FindAsync(albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
            logging
                .Action(nameof(AddPhotos))
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
                .Action(nameof(AddPhotos))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.UPDATE | existingAlbum.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(AddPhotos))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        var existingIds = existingAlbum.Photos
            .Select(photo => photo.PhotoId);

        photoIds = photoIds
            .Where(photoId => photoId > 0)
            .Distinct()
            .Intersect(existingIds)
            .ToArray();

        if (photoIds.Count() <= 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var photosToAdd = await db.Photos
            .Where(photo => photoIds.Contains(photo.Id))
            .Where(photo => (user.Privilege & (photo.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL))) == (photo.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL)))
            .ToListAsync();

        if (photosToAdd.Count() <= 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var newPhotoRelations =
            photosToAdd.Select(photo => new PhotoAlbumRelation() {
                PhotoId = photo.Id,
                Photo = photo,
                AlbumId = existingAlbum.Id,
                Album = existingAlbum,
                Added = DateTime.Now
            })
            .Concat(existingAlbum.Photos)
            .ToList();

        existingAlbum.Photos = newPhotoRelations;
        try
        {
            db.Update(existingAlbum);

            logging
                .Action(nameof(AddPhotos))
                .ExternalTrace(
                    $"The photos in the {nameof(Album)} '{existingAlbum.Title}' (#{existingAlbum.Id}) was just updated.",
                    opts => {
                        opts.SetUser(user);
                    }
                )
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to add photos to the existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(AddPhotos))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to add photos to the existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(AddPhotos))
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
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (<see cref="IEnumerable{int}"/>).
    /// </summary>
    public async Task<ActionResult<Album>> RemovePhotos(int albumId, IEnumerable<int> photoIds)
    {
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

        if (photoIds.Count() > 9999)
        {
            photoIds = photoIds
                .Take(9999)
                .ToArray();
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(RemovePhotos))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums.FindAsync(albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
            logging
                .Action(nameof(RemovePhotos))
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
                .Action(nameof(RemovePhotos))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.UPDATE | existingAlbum.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(RemovePhotos))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        photoIds = photoIds
            .Where(photoId => photoId > 0)
            .Distinct();

        if (photoIds.Count() <= 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var newRelations = existingAlbum.Photos
            .IntersectBy(photoIds, photo => photo.PhotoId)
            .ToList();

        existingAlbum.Photos = newRelations;
        try
        {
            db.Update(existingAlbum);

            logging
                .Action(nameof(RemovePhotos))
                .ExternalTrace(
                    $"The photos in the {nameof(Album)} '{existingAlbum.Title}' (#{existingAlbum.Id}) was just updated.",
                    opts => {
                        opts.SetUser(user);
                    }
                )
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to remove photos from the existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(RemovePhotos))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to remove photos from the existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(RemovePhotos))
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
    /// Update what tags are associated with this <see cref="Album"/> via <paramref name="tags"/> (<see cref="IEnumerable{Reception.Database.Models.Tag}"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<Tag>>> AddTags(int albumId, IEnumerable<Tag> tags)
    {
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999)
                .ToArray();
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(AddTags))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums.FindAsync(albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
            logging
                .Action(nameof(AddTags))
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
            (Privilege.UPDATE | existingAlbum.RequiredPrivilege);

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

        var existingIds = existingAlbum.Tags
            .Select(tag => tag.TagId);

        tags = tags
            .Where(tag => (user.Privilege & (tag.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL))) == (tag.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL)))
            .Distinct()
            .IntersectBy(existingIds, tag => tag.Id)
            .ToList();

        if (tags.Count() <= 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var newTagRelations =
            tags.Select(tag => new AlbumTagRelation() {
                TagId = tag.Id,
                Tag = tag,
                AlbumId = existingAlbum.Id,
                Album = existingAlbum,
                Added = DateTime.Now
            })
            .Concat(existingAlbum.Tags)
            .ToList();

        existingAlbum.Tags = newTagRelations;
        try
        {
            db.Update(existingAlbum);

            logging
                .Action(nameof(AddTags))
                .ExternalTrace(
                    $"The tags of {nameof(Album)} '{existingAlbum.Title}' (#{existingAlbum.Id}) was just updated.",
                    opts => {
                        opts.SetUser(user);
                    }
                )
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to add tags to the existing Album '{existingAlbum.Title}'. ";
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to add tags to the existing Album '{existingAlbum.Title}'. ";
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

        return existingAlbum.Tags
            .Select(relation => relation.Tag)
            .ToArray();
    }

    /// <summary>
    /// Update what tags are associated with this <see cref="Album"/> via <paramref name="tags"/> (<see cref="IEnumerable{Reception.Database.Models.Tag}"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<Tag>>> RemoveTags(int albumId, IEnumerable<Tag> tags)
    {
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999)
                .ToArray();
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(RemoveTags))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Album? existingAlbum = await db.Albums.FindAsync(albumId);

        if (existingAlbum is null)
        {
            string message = $"{nameof(Album)} with ID #{albumId} could not be found!";
            logging
                .Action(nameof(RemoveTags))
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
            (Privilege.UPDATE | existingAlbum.RequiredPrivilege);

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

        var tagIds = tags
            .Select(tag => tag.Id)
            .Distinct();

        if (tagIds.Count() <= 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var newRelations = existingAlbum.Tags
            .IntersectBy(tagIds, tag => tag.TagId)
            .ToList();

        existingAlbum.Tags = newRelations;
        try
        {
            db.Update(existingAlbum);

            logging
                .Action(nameof(RemoveTags))
                .ExternalTrace(
                    $"The tags in {nameof(Album)} '{existingAlbum.Title}' (#{existingAlbum.Id}) was just updated.",
                    opts => {
                        opts.SetUser(user);
                    }
                )
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to remove photos from the existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(RemovePhotos))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to remove photos from the existing Album '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(RemovePhotos))
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

        return existingAlbum.Tags
            .Select(relation => relation.Tag)
            .ToArray();
    }

    /// <summary>
    /// Deletes the <see cref="Reception.Database.Models.Album"/> identified by <paramref name="albumId"/>
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
                .Action(nameof(DeleteAlbum))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte privilegeRequired = (byte)
            (Privilege.DELETE | existingAlbum.RequiredPrivilege);

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(CreateAlbum))
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
            db.Remove(existingAlbum);

            logging
                .Action(nameof(DeleteAlbum))
                .ExternalWarning(
                    $"The {nameof(Album)} ('{existingAlbum.Title}', #{existingAlbum.Id}) was just deleted.",
                    opts => {
                        opts.SetUser(user);
                    }
                )
                .LogAndEnqueue();

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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(Album)} '{existingAlbum.Title}'. ";
            logging
                .Action(nameof(DeleteAlbum))
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
}
