using System.Net;
using Microsoft.AspNetCore.Mvc;
using Reception.Middleware.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Services;

public class TagHandler(
    ILoggingService<TagService> logging,
    IHttpContextAccessor contextAccessor,
    ITagService tagService
) : ITagHandler
{
    /// <summary>
    /// Get all tags.
    /// </summary>
    public async Task<IEnumerable<TagDTO>> GetTags(int? offset = null, int? limit = 9999)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with Unique '<paramref ref="name"/>' (string)
    /// </summary>
    public async Task<ActionResult<TagDTO>> GetTag(string name)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with Primary Key '<paramref ref="tagId"/>' (int)
    /// </summary>
    public async Task<ActionResult<TagDTO>> GetTagById(int tagId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Albums.
    /// </summary>
    public async Task<ActionResult<(Tag, IEnumerable<Album>)>> GetTagAlbums(string name)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Photos.
    /// </summary>
    public async Task<ActionResult<(Tag, IEnumerable<Photo>)>> GetTagPhotos(string name)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create all non-existing tags in the '<paramref ref="tagNames"/>' (string[]) array.
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> CreateTags(IEnumerable<string> tagNames)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create all non-existing tags in the '<paramref ref="tags"/>' (<see cref="IEnumerable{ITag}"/>) array.
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> CreateTags(IEnumerable<ITag> tags)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update the properties of the <see cref="Tag"/> with '<paramref ref="name"/>' (string), *not* its members (i.e Photos or Albums).
    /// </summary>
    public async Task<ActionResult<TagDTO>> UpdateTag(string existingTagName, MutateTag mut)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Edit tags associated with a <see cref="Album"/> identified by PK <paramref name="albumId"/>.
    /// </summary>
    public async Task<ActionResult<(Album, IEnumerable<TagDTO>)>> MutateAlbumTags(int albumId, IEnumerable<ITag> tags)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Edit tags associated with the <paramref name="album"/> (<see cref="Album"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> MutateAlbumTags(Album album, IEnumerable<ITag> tags)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Edit tags associated with a <see cref="Photo"/> identified by PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult<(Photo, IEnumerable<TagDTO>)>> MutatePhotoTags(int photoId, IEnumerable<ITag> tags)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Edit tags associated with the <paramref name="photo"/> (<see cref="Photo"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> MutatePhotoTags(Photo photo, IEnumerable<ITag> tags)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete the <see cref="Tag"/> with '<paramref ref="name"/>' (string).
    /// </summary>
    public async Task<ActionResult> DeleteTag(string name)
    {
        throw new NotImplementedException();
    }
}
