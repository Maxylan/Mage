using Microsoft.AspNetCore.Mvc;
using Reception.Interfaces;
using Reception.Interfaces.DataAccess;
using Reception.Database.Models;
using Reception.Models;
using System.Net;

namespace Reception.Services;

public class CategoryHandler(
    ILoggingService<CategoryHandler> logging,
    ICategoryService categoryService
) : ICategoryHandler
{
    /// <summary>
    /// Get all categories.
    /// Optionally filtered by '<paramref ref="search"/>' (string) and/or paginated with '<paramref ref="offset"/>' (int) &amp; '<paramref ref="limit"/>' (int)
    /// </summary>
    public async Task<IEnumerable<CategoryDTO>> GetCategories(string? search = null, int? offset = null, int? limit = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Category"/> with Primary Key '<paramref ref="categoryId"/>' (int)
    /// </summary>
    public async Task<ActionResult<CategoryDTO>> GetCategory(int categoryId)
    {
        if (categoryId <= 0)
        {
            string message = $"Parameter {nameof(categoryId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(CategoryHandler.GetCategory))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getCategory = await categoryService.GetCategory(categoryId);

        if (getCategory.Value is null)
        {
            string message = $"Failed to find an {nameof(Category)} with ID #{categoryId}.";
            logging
                .Action(nameof(CategoryHandler.GetCategory))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getCategory.Result!;
        }

        return (CategoryDTO)getCategory.Value;
    }

    /// <summary>
    /// Get the <see cref="Category"/> with Unique '<paramref ref="title"/>' (string)
    /// </summary>
    public async Task<ActionResult<CategoryDTO>> GetCategoryByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            string message = $"Parameter {nameof(title)} cannot be null/omitted!";
            logging
                .Action(nameof(CategoryHandler.GetCategoryByTitle))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getCategory = await categoryService.GetCategoryByTitle(title);

        if (getCategory.Value is null)
        {
            string message = $"Failed to find an {nameof(Category)} with {nameof(title)} '{title}'.";
            logging
                .Action(nameof(CategoryHandler.GetCategoryByTitle))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getCategory.Result!;
        }

        return (CategoryDTO)getCategory.Value;
    }

    /// <summary>
    /// Get the <see cref="Category"/> with PK <paramref ref="categoryId"/> (int), along with a collection of all associated Albums.
    /// </summary>
    /// <returns>
    /// <seealso cref="DisplayCategory"/>
    /// </returns>
    public async Task<ActionResult<DisplayCategory>> GetCategoryAlbums(int categoryId)
    {
        if (categoryId <= 0)
        {
            string message = $"Parameter {nameof(categoryId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(CategoryHandler.GetCategoryAlbums))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getCategory = await this.GetCategory(categoryId);
        var category = getCategory.Value;

        if (category is null)
        {
            return getCategory.Result!;
        }

        return new DisplayCategory(category);
    }

    /// <summary>
    /// Create a new <see cref="Category"/>.
    /// </summary>
    public async Task<ActionResult<CategoryDTO>> CreateCategory(MutateCategory mut)
    {
        var newCategory = await categoryService.CreateCategory(mut);

        if (newCategory.Value is null)
        {
            string message = $"Failed to create new {nameof(Category)}!";
            logging
                .Action(nameof(CategoryHandler.CreateCategory))
                .ExternalDebug(message + ".")
                .LogAndEnqueue();

            return newCategory.Result!;
        }

        return (CategoryDTO)newCategory.Value;
    }

    /// <summary>
    /// Update an existing <see cref="Category"/>.
    /// </summary>
    public async Task<ActionResult<CategoryDTO>> UpdateCategory(MutateCategory mut)
    {
        if (mut.Id <= 0)
        {
            string message = $"Parameter {nameof(mut.Id)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(CategoryHandler.UpdateCategory))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var updatedCategory = await categoryService.UpdateCategory(mut);

        if (updatedCategory.Value is null)
        {
            string message = $"Failed to create new {nameof(Category)}!";
            logging
                .Action(nameof(CategoryHandler.UpdateCategory))
                .ExternalDebug(message + ".")
                .LogAndEnqueue();

            return updatedCategory.Result!;
        }

        return (CategoryDTO)updatedCategory.Value;
    }

    /// <summary>
    /// Removes an <see cref="Reception.Database.Models.Album"/> (..identified by PK <paramref name="albumId"/>) from the
    /// <see cref="Reception.Database.Models.Category"/> identified by its PK <paramref name="categoryId"/>.
    /// </summary>
    public async Task<ActionResult> RemoveAlbum(int categoryId, int albumId)
    {
        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(categoryId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(CategoryHandler.RemoveAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(CategoryHandler.RemoveAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var removeAlbumAction = await categoryService.RemoveAlbum(categoryId, albumId);

        if (removeAlbumAction is not OkResult)
        {
            string message = $"Failed to remove {nameof(Album)} #{albumId} from {nameof(Category)} #{categoryId}.";
            logging
                .Action(nameof(CategoryHandler.RemoveAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return removeAlbumAction;
        }

        return removeAlbumAction;
    }

    /// <summary>
    /// Delete the <see cref="Category"/> with PK <paramref ref="categoryId"/> (int).
    /// </summary>
    public async Task<ActionResult> DeleteCategory(int categoryId)
    {
        throw new NotImplementedException();
    }
}
