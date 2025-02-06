using Reception.Models;
using Reception.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface ITagService
{
    /// <summary>
    /// Get all tags.
    /// </summary>
    public abstract Task<IEnumerable<Tag>> GetTags(bool trackEntities = false);

    /// <summary>
    /// Get the <see cref="Tag"/> with Unique '<paramref ref="name"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<Tag>> GetTag(string name);

    /// <summary>
    /// Get the <see cref="Tag"/> with Primary Key '<paramref ref="tagId"/>' (int)
    /// </summary>
    public abstract Task<ActionResult<Tag>> GetTagById(int tagId);

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Albums.
    /// </summary>
    public abstract Task<ActionResult<TagAlbumCollection>> GetTagAlbumCollection(string name);

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Photos.
    /// </summary>
    public abstract Task<ActionResult<TagPhotoCollection>> GetTagPhotoCollection(string name);

    /// <summary>
    /// Create all non-existing tags in the '<paramref ref="tagNames"/>' (string[]) array.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> CreateTags(string[] tagNames);

    /// <summary>
    /// Update the properties of the <see cref="Tag"/> with '<paramref ref="name"/>' (string), *not* its members (i.e Photos or Albums).
    /// </summary>
    public abstract Task<ActionResult<Tag>> UpdateTag(string existingTagName, MutateTag mut);

    /// <summary>
    /// Edit what tags are associated with a <see cref="Album"/> identified by PK <paramref name="albumId"/>.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> MutateAlbumTags(int albumId, string[] tagNames);

    /// <summary>
    /// Edit tags associated with a <see cref="PhotoEntity"/> identified by PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> MutatePhotoTags(int photoId, string[] tagNames);

    /// <summary>
    /// Delete the <see cref="Tag"/> with '<paramref ref="name"/>' (string).
    /// </summary>
    public abstract Task<ActionResult> DeleteTag(string name);
}
