using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Interfaces.DataAccess;

public interface IPublicLinkService
{
    /// <summary>
    /// Get the <see cref="Uri"/> of a <see cref="PublicLink"/>
    /// </summary>
    public abstract Uri GetUri(string code, Dimension? dimension = null);

    /// <summary>
    /// Get the <see cref="Uri"/> of a <paramref name="link"/> (<see cref="PublicLink"/>)
    /// </summary>
    public abstract Uri GetUri(PublicLink link, Dimension? dimension = null);

    /// <summary>
    /// Get the <see cref="PublicLink"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public abstract Task<ActionResult<PublicLink>> GetLink(int linkId);
    /// <summary>
    /// Get the <see cref="PublicLink"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public abstract Task<ActionResult<PublicLink>> GetLinkByCode(string code);

    /// <summary>
    /// Get all <see cref="PublicLink"/> entries.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<PublicLink>>> GetLinks(int limit = 99, int offset = 0);

    /// <summary>
    /// Create a <see cref="PublicLink"/> to the <see cref="Photo"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public virtual Task<ActionResult<PublicLink>> CreateLink(int photoId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return CreateLink(photoId, mutationOptions);
    }
    /// <summary>
    /// Create a <see cref="PublicLink"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public abstract Task<ActionResult<PublicLink>> CreateLink(int photoId, MutateLink mut);

    /// <summary>
    /// Increment the <see cref="Link.Accessed"/> property of a <see cref="PublicLink"/>.
    /// </summary>
    public abstract Task<PublicLink> LinkAccessed(PublicLink link);

    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public virtual Task<ActionResult<PublicLink>> UpdateLink(int linkId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLink(linkId, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public abstract Task<ActionResult<PublicLink>> UpdateLink(int linkId, MutateLink mut);

    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public virtual Task<ActionResult<PublicLink>> UpdateLinkByCode(string code, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLinkByCode(code, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public abstract Task<ActionResult<PublicLink>> UpdateLinkByCode(string code, MutateLink mut);

    /// <summary>
    /// Delete the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public abstract Task<ActionResult> DeleteLink(int linkId);
    /// <summary>
    /// Delete the <see cref="Link"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public abstract Task<ActionResult> DeleteLinkByCode(string code);
}
