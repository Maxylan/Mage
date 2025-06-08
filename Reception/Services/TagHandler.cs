using System.Net;
using Microsoft.AspNetCore.Mvc;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Services;

public class TagHandler(
    ILoggingService<TagHandler> logging,
    ITagService tagService
) : ITagHandler
{
    /// <summary>
    /// Get all tags.
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> GetTags(int? offset = null, int? limit = 9999)
    {
        if (limit is not null && limit <= 0)
        {
            string message = $"Parameter {nameof(limit)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(TagHandler.GetTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (offset is not null && offset < 0)
        {
            string message = $"Parameter {nameof(offset)} has to be a positive integer!";
            logging
                .Action(nameof(TagHandler.GetTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var tags = await tagService.GetTags(offset, limit);

        if (tags is null)
        {
            string message = $"Failed to get {nameof(Tag)}(s)!";
            logging
                .Action(nameof(TagHandler.GetTags))
                .LogDebug(message)
                .LogAndEnqueue();

            return Array.Empty<TagDTO>();
        }

        return tags
            .Select(tag => (TagDTO)tag)
            .ToArray();
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with Unique '<paramref ref="name"/>' (string)
    /// </summary>
    public async Task<ActionResult<TagDTO>> GetTag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = $"Parameter {nameof(name)} cannot be null/omitted!";
            logging
                .Action(nameof(TagHandler.GetTag))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getTag = await tagService.GetTag(name);

        if (getTag.Value is null)
        {
            string message = $"Failed to get {nameof(Tag)} with name '{name}'!";
            logging
                .Action(nameof(TagHandler.GetTags))
                .LogDebug(message)
                .LogAndEnqueue();

            return getTag.Result!;
        }

        return (TagDTO)getTag.Value;
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with Primary Key '<paramref ref="tagId"/>' (int)
    /// </summary>
    public async Task<ActionResult<TagDTO>> GetTagById(int tagId)
    {
        if (tagId <= 0)
        {
            string message = $"Parameter {nameof(tagId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(TagHandler.GetTagById))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getTag = await tagService.GetTagById(tagId);

        if (getTag.Value is null)
        {
            string message = $"Failed to get {nameof(Tag)} with ID #{tagId}!";
            logging
                .Action(nameof(TagHandler.GetTagById))
                .LogDebug(message)
                .LogAndEnqueue();

            return getTag.Result!;
        }

        return (TagDTO)getTag.Value;
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Albums.
    /// </summary>
    public async Task<ActionResult<TagAlbumCollection>> GetTagAlbums(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = $"Parameter {nameof(name)} cannot be null/omitted!";
            logging
                .Action(nameof(TagHandler.GetTagAlbums))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getTagAlbums = await tagService.GetTagAlbums(name);

        if (getTagAlbums.Result is not OkResult &&
            getTagAlbums.Result is not OkObjectResult
        ) {
            string message = $"Failed to get {nameof(Tag)} {nameof(Album)}(s) for the '{name}' {nameof(Tag)}!";
            logging
                .Action(nameof(TagHandler.GetTagAlbums))
                .LogDebug(message)
                .LogAndEnqueue();

            return getTagAlbums.Result!;
        }

        return getTagAlbums;
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Photos.
    /// </summary>
    public async Task<ActionResult<TagPhotoCollection>> GetTagPhotos(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = $"Parameter {nameof(name)} cannot be null/omitted!";
            logging
                .Action(nameof(TagHandler.GetTagPhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getTagAlbums = await tagService.GetTagPhotos(name);

        if (getTagAlbums.Result is not OkResult &&
            getTagAlbums.Result is not OkObjectResult
        ) {
            string message = $"Failed to get {nameof(Tag)} {nameof(Photo)}(s) for the '{name}' {nameof(Tag)}!";
            logging
                .Action(nameof(TagHandler.GetTagPhotos))
                .LogDebug(message)
                .LogAndEnqueue();

            return getTagAlbums.Result!;
        }

        return getTagAlbums;
    }

    /// <summary>
    /// Create all non-existing tags in the '<paramref ref="tagNames"/>' (string[]) array.
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> CreateTags(IEnumerable<string> tagNames)
    {
        if (tagNames.Count() > 9999)
        {
            tagNames = tagNames
                .Take(9999);
        }

        tagNames = tagNames
            .Where(name => string.IsNullOrWhiteSpace(name));

        if (!tagNames.Any())
        {
            string message = $"Parameter {nameof(tagNames)} cannot be empty/omitted!";
            logging
                .Action(nameof(TagHandler.CreateTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var newTags = await tagService.CreateTags(tagNames);

        if (newTags.Value is null)
        {
            string message = $"Failed to get {nameof(Tag)} {nameof(Photo)}(s) for the '{newTags}' {nameof(Tag)}!";
            logging
                .Action(nameof(TagHandler.CreateTags))
                .LogDebug(message)
                .LogAndEnqueue();

            return newTags.Result!;
        }

        return newTags.Value
            .Select(tag => (TagDTO)tag)
            .ToArray();
    }

    /// <summary>
    /// Create all non-existing tags in the '<paramref ref="tags"/>' (<see cref="IEnumerable{ITag}"/>) array.
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> CreateTags(IEnumerable<ITag> tags)
    {
        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999);
        }

        tags = tags
            .Where(tag => string.IsNullOrWhiteSpace(tag.Name));

        if (!tags.Any())
        {
            string message = $"Parameter {nameof(tags)} cannot be empty/omitted!";
            logging
                .Action(nameof(TagHandler.CreateTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var newTags = await tagService.CreateTags(tags);

        if (newTags.Value is null)
        {
            string message = $"Failed to get {nameof(Tag)} {nameof(Photo)}(s) for the '{newTags}' {nameof(Tag)}!";
            logging
                .Action(nameof(TagHandler.CreateTags))
                .LogDebug(message)
                .LogAndEnqueue();

            return newTags.Result!;
        }

        return newTags.Value
            .Select(tag => (TagDTO)tag)
            .ToArray();
    }

    /// <summary>
    /// Update the properties of the <see cref="Tag"/> with '<paramref ref="name"/>' (string), *not* its members (i.e Photos or Albums).
    /// </summary>
    public async Task<ActionResult<TagDTO>> UpdateTag(string existingTagName, MutateTag mut)
    {
        if (string.IsNullOrWhiteSpace(existingTagName))
        {
            string message = $"Parameter {nameof(existingTagName)} cannot be null/omitted!";
            logging
                .Action(nameof(TagHandler.UpdateTag))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var updateTag = await tagService.UpdateTag(existingTagName, mut);

        if (updateTag.Value is null)
        {
            string message = $"Failed to update {nameof(Tag)} with name '{existingTagName}'!";
            logging
                .Action(nameof(TagHandler.UpdateTag))
                .LogDebug(message)
                .LogAndEnqueue();

            return updateTag.Result!;
        }

        return (TagDTO)updateTag.Value;
    }

    /// <summary>
    /// Edit tags associated with a <see cref="Album"/> identified by PK <paramref name="albumId"/>.
    /// </summary>
    public async Task<ActionResult<AlbumTagCollection>> MutateAlbumTags(int albumId, IEnumerable<ITag> tags)
    {
        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(TagHandler.MutateAlbumTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999);
        }

        tags = tags
            .Where(tag => string.IsNullOrWhiteSpace(tag.Name));

        if (!tags.Any())
        {
            string message = $"Parameter {nameof(tags)} cannot be empty/omitted!";
            logging
                .Action(nameof(TagHandler.MutateAlbumTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var mutateTags = await tagService.MutateAlbumTags(albumId, tags);

        if (mutateTags.Result is not OkResult &&
            mutateTags.Result is not OkObjectResult
        ) {
            string message = $"Failed to update {nameof(Tag)}(s) of {nameof(Album)} with ID #{albumId}!";
            logging
                .Action(nameof(TagHandler.MutateAlbumTags))
                .LogDebug(message)
                .LogAndEnqueue();

            return mutateTags.Result!;
        }

        return new AlbumTagCollection(
            (AlbumDTO)mutateTags.Value.Item1,
            mutateTags.Value.Item2.Select(tag => (TagDTO)tag)
        );
    }

    /// <summary>
    /// Edit tags associated with the <paramref name="album"/> (<see cref="Album"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> MutateAlbumTags(Album album, IEnumerable<ITag> tags)
    {
        ArgumentNullException.ThrowIfNull(album);

        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999);
        }

        tags = tags
            .Where(tag => string.IsNullOrWhiteSpace(tag.Name));

        if (!tags.Any())
        {
            string message = $"Parameter {nameof(tags)} cannot be empty/omitted!";
            logging
                .Action(nameof(TagHandler.MutateAlbumTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var mutateTags = await tagService.MutateAlbumTags(album, tags);

        if (mutateTags.Value is null)
        {
            string message = $"Failed to update {nameof(Tag)}(s) of {nameof(Album)} with ID #{album.Id}!";
            logging
                .Action(nameof(TagHandler.MutateAlbumTags))
                .LogDebug(message)
                .LogAndEnqueue();

            return mutateTags.Result!;
        }

        return mutateTags.Value
            .Select(tag => (TagDTO)tag)
            .ToArray();
    }

    /// <summary>
    /// Edit tags associated with a <see cref="Photo"/> identified by PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult<PhotoTagCollection>> MutatePhotoTags(int photoId, IEnumerable<ITag> tags)
    {
        if (photoId <= 0)
        {
            string message = $"Parameter {nameof(photoId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(TagHandler.MutatePhotoTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999);
        }

        tags = tags
            .Where(tag => string.IsNullOrWhiteSpace(tag.Name));

        if (!tags.Any())
        {
            string message = $"Parameter {nameof(tags)} cannot be empty/omitted!";
            logging
                .Action(nameof(TagHandler.MutatePhotoTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var mutateTags = await tagService.MutatePhotoTags(photoId, tags);

        if (mutateTags.Result is not OkResult &&
            mutateTags.Result is not OkObjectResult
        ) {
            string message = $"Failed to update {nameof(Tag)}(s) of {nameof(Photo)} with ID #{photoId}!";
            logging
                .Action(nameof(TagHandler.MutatePhotoTags))
                .LogDebug(message)
                .LogAndEnqueue();

            return mutateTags.Result!;
        }

        return new PhotoTagCollection(
            (PhotoDTO)mutateTags.Value.Item1,
            mutateTags.Value.Item2.Select(tag => (TagDTO)tag)
        );
    }

    /// <summary>
    /// Edit tags associated with the <paramref name="photo"/> (<see cref="Photo"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> MutatePhotoTags(Photo photo, IEnumerable<ITag> tags)
    {
        ArgumentNullException.ThrowIfNull(photo);

        if (tags.Count() > 9999)
        {
            tags = tags
                .Take(9999);
        }

        tags = tags
            .Where(tag => string.IsNullOrWhiteSpace(tag.Name));

        if (!tags.Any())
        {
            string message = $"Parameter {nameof(tags)} cannot be empty/omitted!";
            logging
                .Action(nameof(TagHandler.MutatePhotoTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var mutateTags = await tagService.MutatePhotoTags(photo, tags);

        if (mutateTags.Value is null)
        {
            string message = $"Failed to update {nameof(Tag)}(s) of {nameof(Photo)} with ID #{photo.Id}!";
            logging
                .Action(nameof(TagHandler.MutatePhotoTags))
                .LogDebug(message)
                .LogAndEnqueue();

            return mutateTags.Result!;
        }

        return mutateTags.Value
            .Select(tag => (TagDTO)tag)
            .ToArray();
    }

    /// <summary>
    /// Delete the <see cref="Tag"/> with '<paramref ref="name"/>' (string).
    /// </summary>
    public async Task<ActionResult> DeleteTag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = $"Parameter {nameof(name)} cannot be null/omitted!";
            logging
                .Action(nameof(TagHandler.DeleteTag))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var deleteTag = await tagService.DeleteTag(name);

        if (deleteTag is not OkResult &&
            deleteTag is not OkObjectResult &&
            deleteTag is not NoContentResult
        ) {
            string message = $"Failed to delete {nameof(Tag)} with name '{name}'!";
            logging
                .Action(nameof(TagHandler.DeleteTag))
                .LogDebug(message)
                .LogAndEnqueue();
        }

        return deleteTag;
    }
}
