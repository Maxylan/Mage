using Reception.Models;
using Reception.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface IIntelligenceService
{
    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public abstract Task<ActionResult<OllamaResponse>> InferSourceImage(uint photoId);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public virtual Task<ActionResult<OllamaResponse>> InferSourceImage(PhotoEntity entity) =>
        View(Dimension.SOURCE, entity);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public abstract Task<ActionResult<OllamaResponse>> InferMediumImage(uint photoId);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public virtual Task<ActionResult<OllamaResponse>> InferMediumImage(PhotoEntity entity) =>
        View(Dimension.MEDIUM, entity);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public abstract Task<ActionResult<OllamaResponse>> InferThumbnailImage(uint photoId);

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public virtual Task<ActionResult<OllamaResponse>> InferThumbnailImage(PhotoEntity entity) =>
        View(Dimension.THUMBNAIL, entity);

    /// <summary>
    /// View the <see cref="PhotoEntity"/> (<paramref name="dimension"/>, blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public abstract Task<ActionResult<OllamaResponse>> View(Dimension dimension, PhotoEntity entity);

    /// <summary>
    /// Deliver a <paramref name="prompt"/> to a <paramref name="model"/> (string)
    /// </summary>
    public abstract Task<ActionResult<OllamaResponse>> Chat(string prompt, string model);
}
