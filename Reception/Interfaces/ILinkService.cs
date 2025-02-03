using Microsoft.AspNetCore.Mvc;
using Reception.Models;
using Reception.Models.Entities;

namespace Reception.Interfaces;

public interface ILinkService
{
    /// <summary>
    /// Get the <see cref="Uri"/> of a <see cref="Link"/>
    /// </summary>
    public abstract Uri LinkUri(string code, Dimension? dimension = null);

    /// <summary>
    /// Get all <see cref="Link"/> entries.
    /// <br/><paramref name="includeInactive"/><c> = false</c> can be used to only view active links.
    /// </summary>
    /// <param name="includeInactive">
    /// Passing <c>false</c> would be equivalent to filtering by active links.
    /// </param>
    public abstract Task<ActionResult<IEnumerable<Link>>> GetLinks(bool includeInactive, int limit = 99, int offset = 0);

    /// <summary>
    /// Get the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public abstract Task<ActionResult<Link>> GetLink(int linkId);
    /// <summary>
    /// Get the <see cref="Link"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public abstract Task<ActionResult<Link>> GetLinkByCode(string code);

    /// <summary>
    /// Create a <see cref="Link"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public virtual Task<ActionResult<Link>> CreateLink(int photoId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return CreateLink(photoId, mutationOptions);
    }
    /// <summary>
    /// Create a <see cref="Link"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public abstract Task<ActionResult<Link>> CreateLink(int photoId, MutateLink mut);

    /// <summary>
    /// Increment the <see cref="Link.Accessed"/> property of a <see cref="Link"/>.
    /// </summary>
    public abstract Task<Link> IncrementLinkAccessed(Link link);

    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    public virtual Task<ActionResult<Link>> UpdateLink(int linkId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLink(linkId, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    public abstract Task<ActionResult<Link>> UpdateLink(int linkId, MutateLink mut);

    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    public virtual Task<ActionResult<Link>> UpdateLinkByCode(string code, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLinkByCode(code, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    public abstract Task<ActionResult<Link>> UpdateLinkByCode(string code, MutateLink mut);

    /// <summary>
    /// Delete the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public abstract Task<ActionResult> DeleteLink(int linkId);
    /// <summary>
    /// Delete the <see cref="Link"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public abstract Task<ActionResult> DeleteLinkByCode(string code);
}
