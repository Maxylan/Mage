using Reception.Models;
using Reception.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface IViewService
{
    /// <summary>
    /// View the Source <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    public virtual Task<ActionResult> ViewSource(Guid? code) =>
        View(Dimension.SOURCE, code);

    /// <summary>
    /// View the Medium <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    public virtual Task<ActionResult> ViewMedium(Guid? code) =>
        View(Dimension.MEDIUM, code);

    /// <summary>
    /// View the Medium <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    public virtual Task<ActionResult> ViewThumbnail(Guid? code) =>
        View(Dimension.THUMBNAIL, code);

    /// <summary>
    /// View the <see cref="PhotoEntity"/> (<paramref name="dimension"/>, blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public abstract Task<ActionResult> View(Dimension dimension, Guid? code);
}
