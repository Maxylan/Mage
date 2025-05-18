using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Interfaces.DataAccess;

public interface IIntelligenceService
{
    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public abstract Task<ActionResult<OllamaAnalysis>> InferSourceImage(int photoId, CancellationToken? token = null);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public virtual Task<ActionResult<OllamaAnalysis>> InferSourceImage(Photo entity, CancellationToken? token = null) =>
        View(Dimension.SOURCE, entity, token);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public abstract Task<ActionResult<OllamaAnalysis>> InferMediumImage(int photoId, CancellationToken? token = null);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public virtual Task<ActionResult<OllamaAnalysis>> InferMediumImage(Photo entity, CancellationToken? token = null) =>
        View(Dimension.MEDIUM, entity, token);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public abstract Task<ActionResult<OllamaAnalysis>> InferThumbnailImage(int photoId, CancellationToken? token = null);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public virtual Task<ActionResult<OllamaAnalysis>> InferThumbnailImage(Photo entity, CancellationToken? token = null) =>
        View(Dimension.THUMBNAIL, entity, token);

    /// <summary>
    /// View the <see cref="Photo"/> (<paramref name="dimension"/>, blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public abstract Task<ActionResult<OllamaAnalysis>> View(Dimension dimension, Photo entity, CancellationToken? token = null);

    /// <summary>
    /// Deliver a <paramref name="prompt"/> to a <paramref name="model"/> (string)
    /// </summary>
    public abstract Task<ActionResult<OllamaResponse>> Chat(string prompt, string model, CancellationToken? token = null);

    #region AI Analysis
    /// <summary>
    /// tbd
    /// </summary>
    /// <remarks>
    /// tbd
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public abstract Task<ActionResult<Photo>> ApplyPhotoAnalysis(
        int photoId,
        ActionResult<OllamaAnalysis> imageAnalysis,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// tbd
    /// </summary>
    /// <remarks>
    /// tbd
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public abstract Task<ActionResult<Photo>> ApplyPhotoAnalysis(
        Photo photo,
        ActionResult<OllamaAnalysis> imageAnalysis,
        CancellationToken cancellationToken
    );
    #endregion
}
