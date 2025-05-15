using Reception.Database.Models;
using Reception.Database;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface IViewLinkService
{
    /// <summary>
    /// View the Source <see cref="Photo"/> (blob) associated with the <see cref="PublicLink"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    public virtual Task<ActionResult> ViewSource(Guid? code) =>
        View(Dimension.SOURCE, code);

    /// <summary>
    /// View the Medium <see cref="Photo"/> (blob) associated with the <see cref="PublicLink"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    public virtual Task<ActionResult> ViewMedium(Guid? code) =>
        View(Dimension.MEDIUM, code);

    /// <summary>
    /// View the Medium <see cref="Photo"/> (blob) associated with the <see cref="PublicLink"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    public virtual Task<ActionResult> ViewThumbnail(Guid? code) =>
        View(Dimension.THUMBNAIL, code);

    /// <summary>
    /// View the <see cref="Photo"/> (<paramref name="dimension"/>, blob) associated with the <see cref="PublicLink"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public abstract Task<ActionResult> View(Dimension dimension, Guid? code);
}
