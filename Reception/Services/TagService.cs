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
    ILoggingService logging,
    IHttpContextAccessor contextAccessor,
    IPhotoService photos
) : ITagService
{
    /// <summary>
    /// Get all tags.
    /// </summary>
    public async Task<IEnumerable<Tag>> GetTags(bool trackEntities = false)
    {
        var tags = await db.Tags
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

        List<Tag> newTags = [];
        string[] validTagNames = tagNames
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Where(t => t.Length < 128)
            .Select(t => t.Normalize().Trim())
            .ToArray();

        foreach(string name in tagNames)
        {
            bool exists = await db.Tags.AnyAsync(t => t.Name == name);
            if (exists) {
                continue;
            }

            newTags.Add(new Tag() {
                Name = name
            });

            db.Tags.Add(newTags[^1]);
        }

        if (newTags.Count == 0) {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to add {newTags.Count} new tags. ";
            await logging
                .Action(nameof(CreateTags))
                .InternalError(message + updateException.Message, opts =>
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
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to add {newTags.Count} new tags. ";
            await logging
                .Action(nameof(CreateTags))
                .InternalError(message + ex.Message, opts =>
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

        return newTags;
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
                .InternalError(message + updateException.Message, opts =>
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
                .InternalError(message + ex.Message, opts =>
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
                .InternalError(message + updateException.Message, opts =>
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
                .InternalError(message + ex.Message, opts =>
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
