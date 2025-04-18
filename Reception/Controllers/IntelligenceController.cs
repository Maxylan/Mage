using Reception.Models;
using Reception.Models.Entities;
using Reception.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Constants;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("ai")]
[Produces("application/json")]
public class IntelligenceController(IIntelligenceService handler) : ControllerBase
{
    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    [HttpGet("digest/source/{photoId}")]
    [Tags(ControllerTags.AI)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OllamaResponse>> InferSourceImage(int photoId) =>
        await handler.InferSourceImage(photoId);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    [HttpGet("digest/medium/{photoId}")]
    [Tags(ControllerTags.AI)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OllamaResponse>> InferMediumImage(int photoId) =>
        await handler.InferMediumImage(photoId);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    [HttpGet("digest/thumbnail/{photoId}")]
    [Tags(ControllerTags.AI)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OllamaResponse>> InferThumbnailImage(int photoId) =>
        await handler.InferThumbnailImage(photoId);


    /// <summary>
    /// Deliver a <paramref name="prompt"/> to a <paramref name="model"/> (string)
    /// </summary>
    [HttpPost("chat")]
    [Tags(ControllerTags.AI)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OllamaResponse>> Chat(string prompt, string model) =>
        await handler.Chat(prompt, model);
}
