using System.Net;
using Microsoft.AspNetCore.Mvc;
using Reception.Middleware.Authentication;
using Microsoft.EntityFrameworkCore;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Services.DataAccess;

public class CategoryService(
    MageDb db,
    ILoggingService<CategoryService> logging,
    IHttpContextAccessor contextAccessor
) : ICategoryService
{
    /// <summary>
    /// Get all categories.
    /// Optionally filtered by '<paramref ref="search"/>' (string) and/or paginated with '<paramref ref="offset"/>' (int) &amp; '<paramref ref="limit"/>' (int)
    /// </summary>
    public async Task<IEnumerable<Category>> GetCategories(string? search = null, int? offset = null, int? limit = null)
    {
        IQueryable<Category> categoriesQuery = db.Categories
            .Include(category => category.Albums)
            .ThenInclude(album => new { album.Tags, album.Photos });

        Account? user;
        try
        {
            user = MemoAuth.GetAccount(contextAccessor);

            if (user is null) {
                logging
                    .Action(nameof(GetCategories))
                    .InternalSuspicious("Prevented attempted unauthorized access.")
                    .LogAndEnqueue();

                return [];
            }
        }
        catch (Exception ex)
        {
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(GetCategories))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return [];
        }

        // Filter by privilege
        categoriesQuery = categoriesQuery
            .Where(cat => (user.Privilege & (cat.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL))) == (cat.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL)));

        if (!string.IsNullOrWhiteSpace(search)) {
            categoriesQuery = categoriesQuery
                .Where(category => category.Title.Contains(search));
        }

        if (offset is not null) {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, $"Pagination Parameter {nameof(offset)} has to be a positive integer!");
            }

            categoriesQuery = categoriesQuery
                .Skip(offset.Value);
        }
        if (limit is not null) {
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), limit, $"Pagination Parameter {nameof(limit)} has to be a non-zero positive integer!");
            }

            categoriesQuery = categoriesQuery
                .Take(limit.Value);
        }

        var categories = await categoriesQuery
            .Include(category => category.Albums)
            .ThenInclude(album => new { album.Tags, album.Photos })
            .ToArrayAsync();

        return categories
            .OrderBy(category => category.Title)
            .OrderByDescending(category => category.Albums.Count);
    }

    /// <summary>
    /// Get the <see cref="Category"/> with Primary Key '<paramref ref="categoryId"/>' (int)
    /// </summary>
    public async Task<ActionResult<Category>> GetCategory(int categoryId)
    {
        if (categoryId <= 0)
        {
            string message = $"Parameter {nameof(categoryId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(GetCategory))
                .LogDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        Category? category = await db.Categories.FindAsync(categoryId);

        if (category is null)
        {
            string message = $"Failed to find a {nameof(Category)} matching the given ID #{categoryId}.";
            logging
                .Action(nameof(GetCategory))
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
                .Action(nameof(GetCategory))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredViewPrivilege = (byte)
            (category.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL));

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(GetCategory))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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
                .Action(nameof(GetCategoryByTitle))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredViewPrivilege = (byte)
            (category.RequiredPrivilege & (Privilege.VIEW | Privilege.VIEW_ALL));

        if ((user.Privilege & requiredViewPrivilege) != requiredViewPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredViewPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(GetCategoryByTitle))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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
    /// Create a new <see cref="Category"/>.
    /// </summary>
    public async Task<ActionResult<Category>> CreateCategory(MutateCategory mut)
    {
        ArgumentNullException.ThrowIfNull(mut, nameof(mut));

        if (mut.Albums?.Count() > 9999)
        {
            mut.Albums = mut.Albums
                .Take(9999);
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
                .Action(nameof(CreateCategory))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        byte requiredPrivilege = (byte)
            (Privilege.CREATE | mut.RequiredPrivilege);

        if ((user.Privilege & requiredPrivilege) != requiredPrivilege)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({requiredPrivilege}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(CreateCategory))
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

        bool titleTaken = await db.Categories.AnyAsync(album => album.Title == mut.Title);
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


        List<Album> validAlbums = [];
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
            UpdatedBy = user?.Id,
            Albums = validAlbums
        };

        try
        {
            db.Add(newCategory);

            logging
                .Action(nameof(CreateCategory))
                .InternalTrace($"A new {nameof(Category)} named '{newCategory.Title}' was created.", opts =>
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

        if (mut.Albums?.Count() > 9999)
        {
            mut.Albums = mut.Albums
                .Take(9999);
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
                .Action(nameof(UpdateCategory))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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

        byte privilegeRequired = (byte)
            (Privilege.UPDATE | mut.RequiredPrivilege | mut.RequiredPrivilege);

        if ((mut.RequiredPrivilege & existingCategory.RequiredPrivilege) != existingCategory.RequiredPrivilege) {
            privilegeRequired = (byte)
                (Privilege.ADMIN | privilegeRequired);
        }

        if ((user.Privilege & privilegeRequired) != privilegeRequired)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({privilegeRequired}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(UpdateCategory))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
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
            bool titleTaken = await db.Categories.AnyAsync(album => album.Title == mut.Title);
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

        existingCategory.Title = mut.Title;
        existingCategory.Summary = mut.Summary;
        existingCategory.Description = mut.Description;
        existingCategory.UpdatedAt = DateTime.UtcNow;
        existingCategory.UpdatedBy = user.Id;

        if (mut.Albums is not null) {
            if (mut.Albums?.Any() == true)
            {
                mut.Albums = mut.Albums
                    .Where(photoId => photoId > 0)
                    .ToArray();

                existingCategory.Albums = await db.Albums
                    .Where(album => mut.Albums.Contains(album.Id))
                    .ToListAsync();
            }
            else {
                existingCategory.Albums = [];
            }
        }

        try
        {
            db.Update(existingCategory);

            logging
                .Action(nameof(UpdateCategory))
                .InternalTrace($"An {nameof(Category)} ('{existingCategory.Title}', #{existingCategory.Id}) was just updated.", opts =>
                {
                    opts.SetUser(user);
                });

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
    /// Removes an <see cref="Reception.Database.Models.Album"/> (..identified by PK <paramref name="albumId"/>) from the
    /// <see cref="Reception.Database.Models.Category"/> identified by its PK <paramref name="categoryId"/>.
    /// </summary>
    public async Task<ActionResult> RemoveAlbum(int categoryId, int albumId)
    {
        ArgumentNullException.ThrowIfNull(categoryId, nameof(categoryId));
        ArgumentNullException.ThrowIfNull(albumId, nameof(albumId));

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
                .Action(nameof(RemoveAlbum))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if ((user.Privilege & Privilege.UPDATE) != Privilege.UPDATE)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({Privilege.UPDATE}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(RemoveAlbum))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if (categoryId <= 0)
        {
            string message = $"Parameter '{nameof(categoryId)}' has to be a non-zero positive integer! (Photo ID)";
            logging
                .Action(nameof(RemoveAlbum))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        if (albumId <= 0)
        {
            string message = $"Parameter '{nameof(albumId)}' has to be a non-zero positive integer! (Album ID)";
            logging
                .Action(nameof(RemoveAlbum))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
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
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to remove an album from Category '{existingCategory.Title}'. ";
            logging
                .Action(nameof(RemoveAlbum))
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
                .Action(nameof(DeleteCategory))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if ((user.Privilege & Privilege.DELETE) != Privilege.DELETE)
        {
            string message = $"Prevented action with 'RequiredPrivilege' ({Privilege.DELETE}), which exceeds the user's 'Privilege' of ({user.Privilege}).";
            logging
                .Action(nameof(DeleteCategory))
                .ExternalSuspicious(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        Category? existingCategory = await db.Categories.FindAsync(id);

        if (existingCategory is null)
        {
            string message = $"{nameof(Category)} with ID #{id} could not be found!";
            logging
                .Action(nameof(DeleteCategory))
                .InternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        try
        {
            db.Remove(existingCategory);

            logging
                .Action(nameof(DeleteCategory))
                .InternalWarning(
                    $"The {nameof(Category)} ('{existingCategory.Title}', #{existingCategory.Id}) was just deleted.",
                    opts => {
                        opts.SetUser(user);
                    }
                );

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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(Category)} '{existingCategory.Title}'. ";
            logging
                .Action(nameof(DeleteCategory))
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
