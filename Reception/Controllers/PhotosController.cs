using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Models;
using Reception.Models.Entities;
using Reception.Interfaces;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("photos")]
[Produces("application/json")]
public class PhotosController(IPhotoService handler) : ControllerBase
{
    #region Get single photos.
    /// <summary>
    /// Get a single <see cref="PhotoEntity"/> (entity) by its <paramref name="id"/> (PK, uint).
    /// </summary>
    [HttpGet("entities/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoEntity>> GetPhotoEntity(int id) =>
        await handler.GetPhotoEntity(id);
    
    /// <summary>
    /// Get a single <see cref="PhotoEntity"/> (entity) by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("entities/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoEntity>> GetPhotoEntityBySlug(string slug) =>
        await handler.GetPhotoEntity(slug);
    

    /// <summary>
    /// Get a single <see cref="Photo"/> (single) by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [HttpGet("single/{photo_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Photo>> GetSinglePhoto(int photo_id) =>
        await handler.GetSinglePhoto(photo_id);
    
    /// <summary>
    /// Get a single <see cref="Photo"/> (single) by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("single/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Photo>> GetSinglePhotoBySlug(string slug) =>
        await handler.GetSinglePhoto(slug);
    

    /// <summary>
    /// Get a single <see cref="PhotoCollection"/> by its <paramref name="photo_id"/> (PK, uint).
    /// </summary>
    [HttpGet("{photo_id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoCollection>> GetPhoto(int photo_id) =>
        await handler.GetPhoto(photo_id);
    
    /// <summary>
    /// Get a single <see cref="PhotoCollection"/> by its <paramref name="slug"/> (string).
    /// </summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoCollection>> GetPhotoBySlug(string slug) =>
        await handler.GetPhoto(slug);
    #endregion


    #region Upload photos.
    /// <summary>
    /// Upload a photo/file by streaming it to disk.
    /// </summary>
    [HttpPost("stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoCollection>> StreamPhoto() =>
        await handler.UploadPhoto(opts => {
            // opts.Dimension = Dimension.SOURCE;
            // opts.Slug = ???;
            // opts.Title = ???;
            opts.CreatedAt = DateTime.UtcNow;
            // opts.CreatedBy = ???;
        });
    #endregion
}
