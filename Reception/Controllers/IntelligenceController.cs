using Reception.Models;
using Reception.Database.Models;
using Reception.Interfaces.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http.Timeouts;
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
    [RequestTimeout(milliseconds: 60000)]
    [HttpGet("digest/source/{photoId}")]
    [Tags(ControllerTags.AI)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OllamaAnalysis>> InferSourceImage(int photoId) =>
        await handler.InferSourceImage(photoId);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    [RequestTimeout(milliseconds: 60000)]
    [HttpGet("digest/medium/{photoId}")]
    [Tags(ControllerTags.AI)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OllamaAnalysis>> InferMediumImage(int photoId) =>
        await handler.InferMediumImage(photoId);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    [RequestTimeout(milliseconds: 60000)]
    [HttpGet("digest/thumbnail/{photoId}")]
    [Tags(ControllerTags.AI)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OllamaAnalysis>> InferThumbnailImage(int photoId) =>
        await handler.InferThumbnailImage(photoId);


    /// <summary>
    /// Deliver a <paramref name="prompt"/> to a <paramref name="model"/> (string)
    /// </summary>
    [RequestTimeout(milliseconds: 45000)]
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
