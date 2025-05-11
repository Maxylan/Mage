using Reception.Models;
using Reception.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces.DataAccess;

public interface ICategoryService
{
    /// <summary>
    /// Get all categories.
    /// </summary>
    public abstract Task<IEnumerable<Category>> GetCategories(bool trackEntities = false);

    /// <summary>
    /// Get the <see cref="Category"/> with Primary Key '<paramref ref="id"/>' (int)
    /// </summary>
    public abstract Task<ActionResult<Category>> GetCategory(int id);

    /// <summary>
    /// Get the <see cref="Category"/> with Unique '<paramref ref="title"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<Category>> GetCategoryByTitle(string title);

    /// <summary>
    /// Get the <see cref="Cateogry"/> with '<paramref ref="categoryId"/> (int) along with a collection of all associated Albums.
    /// </summary>
    public abstract Task<ActionResult<CategoryAlbumCollection>> GetCategoryAlbumCollection(int categoryId);

    /// <summary>
    /// Create a new <see cref="Category"/>.
    /// </summary>
    public abstract Task<ActionResult<Category>> CreateCategory(MutateCategory mut);

    /// <summary>
    /// Update an existing <see cref="Category"/>.
    /// </summary>
    public abstract Task<ActionResult<Category>> UpdateCategory(MutateCategory mut);

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
