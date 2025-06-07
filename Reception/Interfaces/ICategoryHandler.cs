using Reception.Models;
using Reception.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface ICategoryHandler
{
    /// <summary>
    /// Get all categories.
    /// Optionally filtered by '<paramref ref="search"/>' (string) and/or paginated with '<paramref ref="offset"/>' (int) &amp; '<paramref ref="limit"/>' (int)
    /// </summary>
    public abstract Task<IEnumerable<CategoryDTO>> GetCategories(string? search = null, int? offset = null, int? limit = null);

    /// <summary>
    /// Get the <see cref="Category"/> with Primary Key '<paramref ref="categoryId"/>' (int)
    /// </summary>
    public abstract Task<ActionResult<CategoryDTO>> GetCategory(int categoryId);

    /// <summary>
    /// Get the <see cref="Category"/> with Unique '<paramref ref="title"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<CategoryDTO>> GetCategoryByTitle(string title);

    /// <summary>
    /// Get the <see cref="Category"/> with PK <paramref ref="categoryId"/> (int), along with a collection of all associated Albums.
    /// </summary>
    /// <returns>
    /// <seealso cref="DisplayCategory"/>
    /// </returns>
    public abstract Task<ActionResult<DisplayCategory>> GetCategoryAlbums(int categoryId);

    /// <summary>
    /// Create a new <see cref="Category"/>.
    /// </summary>
    public abstract Task<ActionResult<CategoryDTO>> CreateCategory(MutateCategory mut);

    /// <summary>
    /// Update an existing <see cref="Category"/>.
    /// </summary>
    public abstract Task<ActionResult<CategoryDTO>> UpdateCategory(MutateCategory mut);

    /// <summary>
    /// Removes an <see cref="Reception.Database.Models.Album"/> (..identified by PK <paramref name="albumId"/>) from the
    /// <see cref="Reception.Database.Models.Category"/> identified by its PK <paramref name="categoryId"/>.
    /// </summary>
    public abstract Task<ActionResult> RemoveAlbum(int categoryId, int albumId);

    /// <summary>
    /// Delete the <see cref="Category"/> with PK <paramref ref="categoryId"/> (int).
    /// </summary>
    public abstract Task<ActionResult> DeleteCategory(int categoryId);
}
