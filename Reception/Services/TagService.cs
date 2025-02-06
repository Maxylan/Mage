using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Authentication;
using Reception.Interfaces;
using Reception.Models;
using Reception.Models.Entities;

namespace Reception.Services;

public class TagService(
    MageDbContext db,
    ILoggingService logging
) : ITagService
{
    /// <summary>
    /// Get all tags.
    /// </summary>
    public async Task<IEnumerable<Tag>> GetTags(bool trackEntities = false)
    {
        var tags = await (
            trackEntities
                ? db.Tags.AsTracking()
                : db.Tags.AsNoTracking()
        )
            .Include(tag => tag.Albums)
            .Include(tag => tag.Photos)
            .ToArrayAsync();

        return tags
            .OrderBy(tag => tag.Name)
            .OrderByDescending(tag => tag.Items);
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with Unique '<paramref ref="name"/>' (string)
    /// </summary>
    public async Task<ActionResult<Tag>> GetTag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = $"{nameof(Tag)} names cannot be null/empty.";
            await logging
                .Action(nameof(GetTag))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        name = name.Trim();
        if (!name.IsNormalized()) {
            name = name.Normalize();
        }

        if (name.Length > 127)
        {
            string message = $"{nameof(Tag)} name exceeds maximum allowed length of 127.";
            await logging
                .Action(nameof(GetTag))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        var tag = await db.Tags
            .Include(tag => tag.Photos)
            .Include(tag => tag.Albums)
            .FirstOrDefaultAsync(tag => tag.Name == name);

        if (tag is null)
        {
            string message = $"{nameof(Tag)} with name '{name}' could not be found!";

            if (Program.IsDevelopment)
            {
                await logging
                    .Action(nameof(GetTag))
                    .InternalDebug(message)
                    .SaveAsync();
            }

            return new NotFoundObjectResult(message);
        }

        return tag;
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with Primary Key '<paramref ref="tagId"/>' (int)
    /// </summary>
    public async Task<ActionResult<Tag>> GetTagById(int tagId)
    {
        if (tagId == default)
        {
            string message = $"{nameof(Tag)} ID has to be a non-zero positive integer!";
            await logging
                .Action(nameof(GetTag))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        var tag = await db.Tags
            .FindAsync(tagId);

        if (tag is null)
        {
            string message = $"{nameof(Tag)} with ID #{tagId} could not be found!";

            if (Program.IsDevelopment)
            {
                await logging
                    .Action(nameof(GetTag))
                    .InternalDebug(message)
                    .SaveAsync();
            }

            return new NotFoundObjectResult(message);
        }

        foreach(var navigation in db.Entry(tag).Navigations)
        {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

        return tag;
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Albums.
    /// </summary>
    public async Task<ActionResult<TagAlbumCollection>> GetTagAlbumCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = $"{nameof(Tag)} names cannot be null/empty.";
            await logging
                .Action(nameof(GetTag))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        name = name.Trim();
        if (!name.IsNormalized()) {
            name = name.Normalize();
        }

        var getTag = await GetTag(name);
        Tag? tag = getTag.Value;

        if (tag is null) {
            return getTag.Result!;
        }

        if (tag.Albums is null || tag.AlbumsCount <= 0)
        {
            if (Program.IsDevelopment) {
                logging.Logger.LogDebug($"Tag {tag.Name} has no associated albums.");
            }

            // Initialize with a collection that's at least empty.
            tag.Albums = [];
        }

        return new TagAlbumCollection(tag);
    }

    /// <summary>
    /// Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Photos.
    /// </summary>
    public async Task<ActionResult<TagPhotoCollection>> GetTagPhotoCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = $"{nameof(Tag)} names cannot be null/empty.";
            await logging
                .Action(nameof(GetTag))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        name = name.Trim();
        if (!name.IsNormalized()) {
            name = name.Normalize();
        }

        var getTag = await GetTag(name);
        Tag? tag = getTag.Value;

        if (tag is null) {
            return getTag.Result!;
        }

        if (tag.Photos is null || tag.PhotosCount <= 0)
        {
            if (Program.IsDevelopment) {
                logging.Logger.LogDebug($"Tag {tag.Name} has no associated photos.");
            }

            // Initialize with a collection that's at least empty.
            tag.Photos = [];
        }

        return new TagPhotoCollection(tag);
    }

    /// <summary>
    /// Create all non-existing tags in the '<paramref ref="tagNames"/>' (string[]) array.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Tag>>> CreateTags(string[] tagNames)
    {
        if (tagNames.Length > 9999)
        {
            tagNames = tagNames
                .Take(9999)
                .ToArray();
        }

        List<Tag> validTags = [];
        int successfullyAddedTags = 0;
        string[] validTagNames = tagNames
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Where(t => t.Length < 128)
            .Select(t => t.Normalize().Trim())
            .ToArray();

        if (validTagNames.Length == 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        foreach(string name in validTagNames)
        {
            Tag? existingTag = await db.Tags.FirstOrDefaultAsync(t => t.Name == name);

            if (existingTag is not null)
            {
                validTags.Add(existingTag);
                continue;
            }

            validTags.Add(new Tag() { Name = name });
            db.Tags.Add(validTags[^1]);
            successfullyAddedTags++;
        }

        if (successfullyAddedTags <= 0) {
            return validTags;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to add {validTags.Count} new tags. ";
            await logging
                .Action(nameof(CreateTags))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to add {validTags.Count} new tags. ";
            await logging
                .Action(nameof(CreateTags))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return validTags;
    }

    /// <summary>
    /// Update the properties of the <see cref="Tag"/> with '<paramref ref="name"/>' (string), *not* its members (i.e Photos or Albums).
    /// </summary>
    public async Task<ActionResult<Tag>> UpdateTag(string existingTagName, MutateTag mut)
    {
        ArgumentNullException.ThrowIfNull(mut);

        if (string.IsNullOrWhiteSpace(mut.Name))
        {
            string message = $"{nameof(Tag)} names cannot be null/empty.";
            await logging
                .Action(nameof(GetTag))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        mut.Name = mut.Name.Trim();
        if (!mut.Name.IsNormalized()) {
            mut.Name = mut.Name.Normalize();
        }

        if (mut.Name.Length > 127)
        {
            string message = $"{nameof(Tag)} name exceeds maximum allowed length of 127.";
            await logging
                .Action(nameof(UpdateTag))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        Tag? tag = null;
        if (mut.Id is not null && mut.Id != 0) {
            tag = await db.Tags.FindAsync(mut.Id);
        }

        if (tag is null)
        {
            var getTag = await GetTag(existingTagName);
            tag = getTag.Value;

            if (tag is null)
            {
                string message = $"{nameof(Tag)} named '{existingTagName}' or identified by ID #{mut.Id} could not be found!";

                /* if (Program.IsDevelopment)
                {   // A failure *should* already be logged at `GetTag`
                    await logging
                        .Action(nameof(UpdateTag))
                        .InternalDebug(message)
                        .SaveAsync();
                } */

                return getTag.Result!;
            }
        }

        tag.Name = mut.Name;
        tag.Description = mut.Description;

        try
        {
            db.Update(tag);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update {nameof(Tag)} '{existingTagName}'. ";
            await logging
                .Action(nameof(UpdateTag))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update {nameof(Tag)} '{existingTagName}'. ";
            await logging
                .Action(nameof(UpdateTag))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return tag;
    }

    /// <summary>
    /// Edit tags associated with a <see cref="Album"/> identified by PK <paramref name="albumId"/>.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Tag>>> MutateAlbumTags(int albumId, string[] tagNames)
    {
        if (albumId <= 0) {
            throw new ArgumentException($"Parameter {nameof(albumId)} has to be a non-zero positive integer!", nameof(albumId));
        }

        Album? album = await db.Albums.FindAsync(albumId);

        if (album is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} with {nameof(albumId)} #{albumId}.";
            await logging
                .Action(nameof(MutateAlbumTags))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        // Load missing navigation entries.
        foreach (var navigation in db.Entry(album).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        var createAndSanitizeTags = await CreateTags(tagNames);
        var validTags = createAndSanitizeTags.Value;

        if (validTags is null /* || createAndSanitizeTags.Result is not OkObjectResult */) {
            return createAndSanitizeTags.Result!;
        }

        foreach(var navigation in db.Entry(album).Navigations)
        {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

        if (album.AlbumTags.Intersect(validTags).Count() == validTags.Count()) { // ..no change
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        album.AlbumTags = validTags.ToList();

        try
        {
            db.Update(album);

            if (Program.IsDevelopment)
            {
                logging
                    .Action(nameof(MutateAlbumTags))
                    .InternalDebug($"The tags associated with {nameof(Album)} '{album.Title}' (#{album.Id}) was just updated.");
            }

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update the tags of a {nameof(Album)} with ID #{albumId}. ";
            await logging
                .Action(nameof(MutateAlbumTags))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update tags of a {nameof(Album)} with ID #{albumId}. ";
            await logging
                .Action(nameof(MutateAlbumTags))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return album.AlbumTags.ToArray();
    }

    /// <summary>
    /// Edit tags associated with a <see cref="PhotoEntity"/> identified by PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult<IEnumerable<Tag>>> MutatePhotoTags(int photoId, string[] tagNames)
    {
        if (photoId <= 0) {
            throw new ArgumentException($"Parameter {nameof(photoId)} has to be a non-zero positive integer!", nameof(photoId));
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} with {nameof(photoId)} #{photoId}.";
            await logging
                .Action(nameof(MutatePhotoTags))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        // Load missing navigation entries.
        foreach (var navigation in db.Entry(photo).Navigations)
        {
            if (!navigation.IsLoaded)
            {
                await navigation.LoadAsync();
            }
        }

        var createAndSanitizeTags = await CreateTags(tagNames);
        var validTags = createAndSanitizeTags.Value;

        if (validTags is null /* || createAndSanitizeTags.Result is not OkObjectResult */) {
            return createAndSanitizeTags.Result!;
        }

        foreach(var navigation in db.Entry(photo).Navigations)
        {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

        if (photo.Tags.Intersect(validTags).Count() == validTags.Count()) { // ..no change
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        photo.Tags = validTags.ToList();

        try
        {
            db.Update(photo);

            if (Program.IsDevelopment)
            {
                logging
                    .Action(nameof(MutatePhotoTags))
                    .InternalDebug($"The tags associated with {nameof(PhotoEntity)} '{photo.Title}' (#{photo.Id}) was just updated.");
            }

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update the tags of a {nameof(PhotoEntity)} with ID #{photoId}. ";
            await logging
                .Action(nameof(MutatePhotoTags))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update tags of a {nameof(PhotoEntity)} with ID #{photoId}. ";
            await logging
                .Action(nameof(MutatePhotoTags))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return photo.Tags.ToArray();
    }

    /// <summary>
    /// Delete the <see cref="Tag"/> with '<paramref ref="name"/>' (string).
    /// </summary>
    public async Task<ActionResult> DeleteTag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = $"{nameof(Tag)} names cannot be null/empty.";
            await logging
                .Action(nameof(GetTag))
                .InternalDebug(message)
                .SaveAsync();

            return new BadRequestObjectResult(message);
        }

        name = name.Trim();
        if (!name.IsNormalized()) {
            name = name.Normalize();
        }

        var getTag = await GetTag(name);
        Tag? tag = getTag.Value;

        if (tag is null)
        {
            string message = $"{nameof(Tag)} named '{name}' could not be found!";

            /* if (Program.IsDevelopment)
            {   // A failure *should* already be logged at `GetTag`
                await logging
                    .Action(nameof(DeleteTag))
                    .InternalDebug(message)
                    .SaveAsync();
                } */

            return getTag.Result!;
        }

        try
        {
            db.Remove(tag);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(Tag)} '{name}'. ";
            await logging
                .Action(nameof(DeleteTag))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(Tag)} '{name}'. ";
            await logging
                .Action(nameof(DeleteTag))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return new NoContentResult();
    }
}
