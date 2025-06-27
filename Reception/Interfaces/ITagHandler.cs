using Reception.Models;
using Reception.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface ITagHandler
{
    /// <summary>
    /// Get all tags.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> GetTags(int? offset = null, int? limit = 9999);

    /// <summary>
    /// Get the <see cref="Tag"/> with Unique '<paramref ref="name"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<TagDTO>> GetTag(string name);

    /// <summary>
    /// Get all tags (<see cref="Tag"/>) matching names in '<paramref ref="tagNames"/>' (string[])
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> GetTagsByNames(IEnumerable<string> tagNames);

    /// <summary>
    /// Get the <see cref="Tag"/> with Primary Key '<paramref ref="tagId"/>' (int)
    /// </summary>
    public abstract Task<ActionResult<TagDTO>> GetTagById(int tagId);

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Albums.
    /// </summary>
    public abstract Task<ActionResult<TagAlbumCollection>> GetTagAlbums(string name);

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Photos.
    /// </summary>
    public abstract Task<ActionResult<TagPhotoCollection>> GetTagPhotos(string name);

    /// <summary>
    /// Create all non-existing tags in the '<paramref ref="tagNames"/>' (string[]) array.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> CreateTags(IEnumerable<string> tagNames);

    /// <summary>
    /// Create all non-existing tags in the '<paramref ref="tags"/>' (<see cref="IEnumerable{ITag}"/>) array.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> CreateTags(IEnumerable<ITag> tags);

    /// <summary>
    /// Update the properties of the <see cref="Tag"/> with '<paramref ref="name"/>' (string), *not* its members (i.e Photos or Albums).
    /// </summary>
    public abstract Task<ActionResult<TagDTO>> UpdateTag(string existingTagName, MutateTag mut);

    /// <summary>
    /// Edit what tags are associated with a <see cref="Album"/> identified by PK <paramref name="albumId"/>.
    /// </summary>
    public abstract Task<ActionResult<AlbumTagCollection>> MutateAlbumTags(int albumId, IEnumerable<ITag> tags);

    /// <summary>
    /// Edit tags associated with the <paramref name="album"/> (<see cref="Album"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> MutateAlbumTags(Album album, IEnumerable<ITag> tags);

    /// <summary>
    /// Edit tags associated with a <see cref="Photo"/> identified by PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult<PhotoTagCollection>> MutatePhotoTags(int photoId, IEnumerable<ITag> tags);

    /// <summary>
    /// Edit tags associated with the <paramref name="photo"/> (<see cref="Photo"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> MutatePhotoTags(Photo photo, IEnumerable<ITag> tags);

    /// <summary>
    /// Delete the <see cref="Tag"/> with '<paramref ref="name"/>' (string).
    /// </summary>
    public abstract Task<ActionResult> DeleteTag(string name);
}
