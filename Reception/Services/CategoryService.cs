using Reception.Authentication;
using Reception.Interfaces;
using Reception.Models;
using Reception.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Reception.Services;

public class CategoryService(
    MageDbContext db,
    ILoggingService<CategoryService> logging,
    IHttpContextAccessor contextAccessor
) : ICategoryService
{
    /// <summary>
    /// Get all categories.
    /// </summary>
    public async Task<IEnumerable<Category>> GetCategories(bool trackEntities = false)
    {
        var categories = await (
            trackEntities
                ? db.Categories.AsTracking()
                : db.Categories.AsNoTracking()
        )
            .Include(category => category.Albums)
            .ThenInclude(album => new { album.Tags, album.Photos })
            .ToArrayAsync();

        return categories
            .OrderBy(category => category.Title)
            .OrderByDescending(category => category.Items);
    }

    /// <summary>
    /// Get the <see cref="Category"/> with Primary Key '<paramref ref="id"/>' (int)
    /// </summary>
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException($"Parameter {nameof(id)} has to be a non-zero positive integer!", nameof(id));
        }

        Category? category = await db.Categories.FindAsync(id);

        if (category is null)
        {
            string message = $"Failed to find a {nameof(Category)} matching the given {nameof(id)} #{id}.";
            logging
                .Action(nameof(GetCategory))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        // Load missing navigation entries.
        foreach (var navigation in db.Entry(category).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        foreach (var album in category.Albums)
        {
            foreach (var navigation in db.Entry(album).Navigations)
            {
                if (!navigation.IsLoaded)
                {
                    await navigation.LoadAsync();
                }
            }
        }

        return category;
    }

    /// <summary>
    /// Get the <see cref="Category"/> with Unique '<paramref ref="title"/>' (string)
    /// </summary>
    public async Task<ActionResult<Category>> GetCategoryByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            string message = $"{nameof(Category)} titles cannot be null/empty.";
            logging
                .Action(nameof(GetCategoryByTitle))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        title = title.Trim();
        if (!title.IsNormalized()) {
            title = title.Normalize();
        }

        if (title.Length > 255)
        {
            string message = $"{nameof(Category)} title exceeds maximum allowed length of 255.";
            logging
                .Action(nameof(GetCategoryByTitle))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        var category = await db.Categories
            .Include(category => category.Albums)
            .FirstOrDefaultAsync(category => category.Title == title);

        if (category is null)
        {
            string message = $"{nameof(Category)} with title '{title}' could not be found!";
            logging
                .Action(nameof(GetCategoryByTitle))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        return category;
    }

    /// <summary>
    /// Get the <see cref="Cateogry"/> with '<paramref ref="categoryId"/> (int) along with a collection of all associated Albums.
    /// </summary>
    public async Task<ActionResult<CategoryAlbumCollection>> GetCategoryAlbumCollection(int categoryId)
    {
        var getCategory = await GetCategory(categoryId);
        var category = getCategory.Value;

        if (category is null) {
            return getCategory.Result!;
        }

        return new CategoryAlbumCollection(category);
    }

    /// <summary>
    /// Create a new <see cref="Category"/>.
    /// </summary>
    public async Task<ActionResult<Category>> CreateCategory(MutateCategory mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));

        if (mut.Albums?.Length > 9999)
        {
            mut.Albums = mut.Albums
                .Take(9999)
                .ToArray();
        }

        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(CreateCategory)} Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(CreateCategory))
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
                    .Action(nameof(CreateCategory))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .LogAndEnqueue();
            }
        }

        if (string.IsNullOrWhiteSpace(mut.Title))
        {
            string message = $"Parameter '{nameof(mut.Title)}' may not be null/empty!";
            logging
                .Action(nameof(CreateCategory))
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
            string message = $"{nameof(Category.Title)} exceeds maximum allowed length of 255.";
            logging
                .Action(nameof(CreateCategory))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        bool titleTaken = await db.Albums.AnyAsync(album => album.Title == mut.Title);
        if (titleTaken)
        {
            string message = $"{nameof(Category.Title)} was already taken!";
            logging
                .Action(nameof(CreateCategory))
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
                string message = $"{nameof(Category.Summary)} exceeds maximum allowed length of 255.";
                logging
                    .Action(nameof(CreateCategory))
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


        List<Album>? validAlbums = null;
        if (mut.Albums?.Any() == true)
        {
            mut.Albums = mut.Albums
                .Where(albumId => albumId > 0)
                .ToArray();

            validAlbums = await db.Albums
                .Where(album => mut.Albums.Contains(album.Id))
                .ToListAsync();
        }

        Category newCategory = new()
        {
            Title = mut.Title,
            Summary = mut.Summary,
            Description = mut.Description,
            CreatedBy = user?.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Albums = validAlbums ?? []
        };

        try
        {
            db.Add(newCategory);

            logging
                .Action(nameof(CreateCategory))
                .InternalInformation($"A new {nameof(Category)} named '{newCategory.Title}' was created.", opts =>
                {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to create new Category '{newCategory.Title}'. ";
            logging
                .Action(nameof(CreateCategory))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to create new Category '{newCategory.Title}'. ";
            logging
                .Action(nameof(CreateCategory))
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

        return newCategory;
    }

    /// <summary>
    /// Update an existing <see cref="Category"/>.
    /// </summary>
    public async Task<ActionResult<Category>> UpdateCategory(MutateCategory mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));
        ArgumentNullException.ThrowIfNull(mut.Id, nameof(mut.Id));

        if (mut.Albums?.Length > 9999)
        {
            mut.Albums = mut.Albums
                .Take(9999)
                .ToArray();
        }

        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(UpdateCategory)} Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(UpdateCategory))
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
                    .Action(nameof(UpdateCategory))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .LogAndEnqueue();
            }
        }

        if (mut.Id <= 0)
        {
            string message = $"Parameter '{nameof(mut.Id)}' has to be a non-zero positive integer! (Category ID)";
            logging
                .Action(nameof(UpdateCategory))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Category? existingCategory = await db.Categories.FindAsync(mut.Id);

        if (existingCategory is null)
        {
            string message = $"{nameof(Category)} with ID #{mut.Id} could not be found!";
            logging
                .Action(nameof(UpdateCategory))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        foreach(var navigation in db.Entry(existingCategory).Navigations)
        {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

        if (string.IsNullOrWhiteSpace(mut.Title))
        {
            string message = $"Parameter '{nameof(mut.Title)}' may not be null/empty!";
            logging
                .Action(nameof(UpdateCategory))
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
            string message = $"{nameof(Category.Title)} exceeds maximum allowed length of 255.";
            logging
                .Action(nameof(UpdateCategory))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (mut.Title != existingCategory.Title)
        {
            bool titleTaken = await db.Albums.AnyAsync(album => album.Title == mut.Title);
            if (titleTaken)
            {
                string message = $"{nameof(Category.Title)} was already taken!";
                logging
                    .Action(nameof(UpdateCategory))
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
                string message = $"{nameof(Category.Summary)} exceeds maximum allowed length of 255.";
                logging
                    .Action(nameof(UpdateCategory))
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

        List<Album>? validAlbums = null;
        if (mut.Albums?.Any() == true)
        {
            mut.Albums = mut.Albums
                .Where(photoId => photoId > 0)
                .ToArray();

            validAlbums = await db.Albums
                .Where(album => mut.Albums.Contains(album.Id))
                .ToListAsync();
        }

        existingCategory.Title = mut.Title;
        existingCategory.Summary = mut.Summary;
        existingCategory.Description = mut.Description;
        existingCategory.UpdatedAt = DateTime.UtcNow;

        if (mut.Albums is not null) {
            existingCategory.Albums = validAlbums ?? [];
        }

        try
        {
            db.Update(existingCategory);

            if (Program.IsDevelopment)
            {
                logging
                    .Action(nameof(UpdateCategory))
                    .InternalDebug($"An {nameof(Category)} ('{existingCategory.Title}', #{existingCategory.Id}) was just updated.", opts =>
                    {
                        opts.SetUser(user);
                    });
            }

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update existing Category '{existingCategory.Title}'. ";
            logging
                .Action(nameof(UpdateCategory))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update existing Category '{existingCategory.Title}'. ";
            logging
                .Action(nameof(UpdateCategory))
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

        return existingCategory;
    }

    /// <summary>
    /// Removes an <see cref="Reception.Models.Entities.Album"/> (..identified by PK <paramref name="albumId"/>) from the
    /// <see cref="Reception.Models.Entities.Category"/> identified by its PK <paramref name="categoryId"/>.
    /// </summary>
    public async Task<ActionResult> RemoveAlbum(int categoryId, int albumId)
    {
        ArgumentNullException.ThrowIfNull(categoryId, nameof(categoryId));
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

        if (categoryId <= 0)
        {
            string message = $"Parameter '{nameof(categoryId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(RemoveAlbum))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(RemoveAlbum))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Category? existingCategory = await db.Categories
            .Include(category => category.Albums)
            .FirstOrDefaultAsync(category => category.Id == categoryId);

        if (existingCategory is null)
        {
            string message = $"{nameof(Category)} with ID #{categoryId} could not be found!";
            logging
                .Action(nameof(RemoveAlbum))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        Album? albumToRemove = existingCategory.Albums
            .FirstOrDefault(album => album.Id == albumId);

        if (albumToRemove is null) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        existingCategory.Albums.Remove(albumToRemove);

        try
        {
            db.Update(existingCategory);

            logging
                .Action(nameof(RemoveAlbum))
                .InternalTrace($"A photo was just removed from {nameof(Album)} ('{existingCategory.Title}', #{existingCategory.Id})");

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to remove an album from Category '{existingCategory.Title}'. ";
            logging
                .Action(nameof(RemoveAlbum))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to remove an album from Category '{existingCategory.Title}'. ";
            logging
                .Action(nameof(RemoveAlbum))
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
    /// Delete the <see cref="Tag"/> with '<paramref ref="name"/>' (string).
    /// </summary>
    public async Task<ActionResult> DeleteCategory(int id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));

        if (id <= 0)
        {
            string message = $"Parameter '{nameof(id)}' has to be a non-zero positive integer! (Category ID)";
            logging
                .Action(nameof(DeleteCategory))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        Category? existingCategory = await db.Categories.FindAsync(id);

        if (existingCategory is null)
        {
            string message = $"{nameof(Category)} with ID #{id} could not be found!";
            logging
                .Action(nameof(DeleteCategory))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        try
        {
            db.Remove(existingCategory);

            logging
                .Action(nameof(DeleteCategory))
                .InternalWarning($"The {nameof(Category)} ('{existingCategory.Title}', #{existingCategory.Id}) was just deleted.");

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(Category)} '{existingCategory.Title}'. ";
            logging
                .Action(nameof(DeleteCategory))
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(Category)} '{existingCategory.Title}'. ";
            logging
                .Action(nameof(DeleteCategory))
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
