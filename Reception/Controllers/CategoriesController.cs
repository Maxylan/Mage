using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Models;
using Reception.Database.Models;
using Reception.Interfaces;
using Reception.Utilities;
using Reception.Constants;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("categories")]
[Produces("application/json")]
public class CategoriesController(ICategoryHandler handler) : ControllerBase
{
    /// <summary>
    /// Get all categories.
    /// </summary>
    [HttpGet]
    [Tags(ControllerTags.CATEGORIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories() => Ok(
        await handler.GetCategories()
    );

    /// <summary>
    /// Get the <see cref="Category"/> with Primary Key '<paramref ref="id"/>' (int)
    /// </summary>
    [HttpGet("{id:int}")]
    [Tags(ControllerTags.CATEGORIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDTO>> GetCategory(int id) =>
        await handler.GetCategory(id);

    /// <summary>
    /// Get the <see cref="Category"/> with unique '<paramref ref="title"/>' (string)
    /// </summary>
    [HttpGet("title/{title}")]
    [Tags(ControllerTags.CATEGORIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDTO>> GetCategoryByTitle(string title) =>
        await handler.GetCategoryByTitle(title);

    /// <summary>
    /// Get the <see cref="Category"/> with PK <paramref ref="category_id"/> (int), along with a collection of all associated Albums.
    /// </summary>
    /// <returns>
    /// <seealso cref="DisplayCategory"/>
    /// </returns>
    [HttpGet("{category_id:int}/albums")]
    [Tags(ControllerTags.CATEGORIES, ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayCategory>> GetCategoryAlbums(int category_id) =>
        await handler.GetCategoryAlbums(category_id);

    /// <summary>
    /// Create a new <see cref="Category"/>.
    /// </summary>
    [HttpPost]
    [Tags(ControllerTags.CATEGORIES)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDTO>> CreateCategory(MutateCategory mut) =>
        await handler.CreateCategory(mut);

    /// <summary>
    /// Update the properties of a <see cref="Category"/>.
    /// </summary>
    [HttpPut]
    [Tags(ControllerTags.CATEGORIES)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDTO>> UpdateCategory(MutateCategory mut) =>
        await handler.UpdateCategory(mut);

    /// <summary>
    /// Remove a single <see cref="Album"/> (<paramref name="album_id"/>, int) ..from a single <see cref="Category"/> identified by PK '<paramref ref="category_id"/>' (int)
    /// </summary>
    [HttpPatch("{category_id:int}/remove/album/{album_id:int}")]
    [Tags(ControllerTags.CATEGORIES, ControllerTags.ALBUMS)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveAlbum(int category_id, int album_id) =>
        await handler.RemoveAlbum(category_id, album_id);

    /// <summary>
    /// Delete the <see cref="Category"/> with PK <paramref ref="category_id"/> (int).
    /// </summary>
    [HttpDelete("{category_id:int}")]
    [Tags(ControllerTags.CATEGORIES)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCategory(int category_id) =>
        await handler.DeleteCategory(category_id);
}
