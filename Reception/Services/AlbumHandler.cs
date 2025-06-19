using System.Net;
using Reception.Middleware.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Services;

public class AlbumHandler(
    ILoggingService<AlbumHandler> logging,
    IHttpContextAccessor contextAccessor,
    IAlbumService albumService
) : IAlbumHandler
{
    /// <summary>
    /// Get the <see cref="Album"/> with Primary Key '<paramref ref="albumId"/>'
    /// </summary>
    public async Task<ActionResult<DisplayAlbum>> GetAlbum(int albumId)
    {
        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(AlbumHandler.GetAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getAlbum = await albumService.GetAlbum(albumId);

        if (getAlbum.Value is null)
        {
            string message = $"Failed to find an {nameof(Album)} with ID #{albumId}.";
            logging
                .Action(nameof(AlbumHandler.GetAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getAlbum.Result!;
        }

        return new DisplayAlbum(getAlbum.Value);
    }

    /// <summary>
    /// Get all <see cref="DisplayAlbum"/> instances matching a range of optional filtering / pagination options (<seealso cref="FilterAlbumsOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<DisplayAlbum>>> FilterAlbums(Action<FilterAlbumsOptions> opts)
    {
        FilterAlbumsOptions filtering = new();
        opts(filtering);

        return FilterAlbums(filtering);
    }

    /// <summary>
    /// Get all <see cref="DisplayAlbum"/> instances matching a range of optional filtering / pagination options (<seealso cref="FilterAlbumsOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<DisplayAlbum>>> FilterAlbums(FilterAlbumsOptions filter)
    {
        throw new NotImplementedException(nameof(SearchForAlbums));
    }

    /// <summary>
    /// Get all <see cref="DisplayAlbum"/> instances by evaluating a wide range of optional search / pagination options (<seealso cref="AlbumSearchQuery"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<DisplayAlbum>>> SearchForAlbums(Action<AlbumSearchQuery> opts)
    {
        AlbumSearchQuery filtering = new();
        opts(filtering);

        return SearchForAlbums(filtering);
    }

    /// <summary>
    /// Get all <see cref="DisplayAlbum"/> instances by evaluating a wide range of optional search / pagination options (<seealso cref="AlbumSearchQuery"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<DisplayAlbum>>> SearchForAlbums(AlbumSearchQuery filter)
    {
        throw new NotImplementedException(nameof(SearchForAlbums));
    }

    /// <summary>
    /// Create a new <see cref="Album"/>.
    /// </summary>
    public async Task<ActionResult<DisplayAlbum>> CreateAlbum(MutateAlbum mut)
    {
        var createAlbum = await albumService.CreateAlbum(mut);

        if (createAlbum.Value is null)
        {
            string message = $"Failed to update {nameof(Album)} '{mut.Title}'.";
            logging
                .Action(nameof(AlbumHandler.CreateAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return createAlbum.Result!;
        }

        return new DisplayAlbum(createAlbum.Value);
    }

    /// <summary>
    /// Updates an <see cref="Album"/> in the database.
    /// </summary>
    public async Task<ActionResult<DisplayAlbum>> UpdateAlbum(MutateAlbum mut)
    {
        var updateAlbum = await albumService.UpdateAlbum(mut);

        if (updateAlbum.Value is null)
        {
            string message = $"Failed to update new {nameof(Album)} '{mut.Title}'";
            logging
                .Action(nameof(AlbumHandler.UpdateAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return updateAlbum.Result!;
        }

        return new DisplayAlbum(updateAlbum.Value);
    }

    /// <summary>
    /// Add <see cref="Tag"/>(s) (<paramref name="tags"/>) ..to a <see cref="Album"/> identified by PK '<paramref ref="albumId"/>' (int)
    /// </summary>
    public async Task<ActionResult> ToggleFavorite(int albumId)
    {
        Account? user;
        try
        {
            user = MemoAuth.GetAccount(contextAccessor);

            if (user is null) {
                return new ObjectResult("Prevented attempted unauthorized access.") {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
        catch (Exception ex)
        {
            string message = $"Cought an '{ex.GetType().FullName}' invoking {nameof(MemoAuth.GetAccount)}!";
            logging
                .Action(nameof(AlbumHandler.ToggleFavorite))
                .ExternalError(message, opts => { opts.Exception = ex; })
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.Forbidden.ToString() : message) {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(AlbumHandler.ToggleFavorite))
                .ExternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var toggleFavoriteResult = await albumService.ToggleFavorite(albumId);

        if (toggleFavoriteResult is not OkResult)
        {
            string message = $"Failed to toggle favorite-status on {nameof(Album)} #{albumId} for user '{user.Id}'.";
            logging
                .Action(nameof(AlbumHandler.ToggleFavorite))
                .ExternalDebug(message, opts => {
                    opts.SetUser(user);
                })
                .LogAndEnqueue();
        }

        return toggleFavoriteResult;
    }

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (<see cref="IEnumerable{int}"/>).
    /// </summary>
    public async Task<ActionResult<DisplayAlbum>> AddPhotos(int albumId, IEnumerable<int> photoIds)
    {
        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(AlbumHandler.AddPhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (photoIds is null || photoIds.Count() == 0)
        {
            string message = $"Parameter {nameof(photoIds)} was omitted, no changes made.";
            logging
                .Action(nameof(AlbumHandler.AddPhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var updateAlbum = await albumService.AddPhotos(albumId, photoIds);

        if (updateAlbum.Value is null)
        {
            string message = $"Failed to add {photoIds.Count()} photos to {nameof(Album)} #{albumId}'.";
            logging
                .Action(nameof(AlbumHandler.UpdateAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return updateAlbum.Result!;
        }

        return new DisplayAlbum(updateAlbum.Value);
    }

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (<see cref="IEnumerable{int}"/>).
    /// </summary>
    public async Task<ActionResult<DisplayAlbum>> RemovePhotos(int albumId, IEnumerable<int> photoIds)
    {
        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(AlbumHandler.RemovePhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (photoIds is null || photoIds.Count() == 0)
        {
            string message = $"Parameter {nameof(photoIds)} was omitted, no changes made.";
            logging
                .Action(nameof(AlbumHandler.RemovePhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var updateAlbum = await albumService.RemovePhotos(albumId, photoIds);

        if (updateAlbum.Value is null)
        {
            string message = $"Failed to remove {photoIds.Count()} photos from {nameof(Album)} #{albumId}.";
            logging
                .Action(nameof(AlbumHandler.RemovePhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return updateAlbum.Result!;
        }

        return new DisplayAlbum(updateAlbum.Value);
    }

    /// <summary>
    /// Update what tags are associated with this <see cref="Album"/> via <paramref name="tags"/> (<see cref="IEnumerable{Reception.Database.Models.ITag}"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> AddTags(int albumId, IEnumerable<ITag> tags)
    {
        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(AlbumHandler.AddTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (tags is null || tags.Count() == 0)
        {
            string message = $"Parameter '{nameof(tags)}' was omitted, no changes made.";
            logging
                .Action(nameof(AlbumHandler.AddTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var newTags = tags.Select(t => (Tag)t);
        var albumTags = await albumService.AddTags(albumId, newTags);

        if (albumTags.Value is null)
        {
            string message = $"Failed to add {tags.Count()} tags to {nameof(Album)} #{albumId}.";
            logging
                .Action(nameof(AlbumHandler.AddTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return albumTags.Result!;
        }

        return albumTags.Value
            .Select(t => (TagDTO)t)
            .ToArray();
    }

    /// <summary>
    /// Update what tags are associated with this <see cref="Album"/> via <paramref name="tags"/> (<see cref="IEnumerable{Reception.Database.Models.ITag}"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> RemoveTags(int albumId, IEnumerable<ITag> tags)
    {
        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(AlbumHandler.RemoveTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (tags is null || tags.Count() == 0)
        {
            string message = $"Parameter '{nameof(tags)}' was omitted, no changes made.";
            logging
                .Action(nameof(AlbumHandler.RemoveTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }

        var tagsToRemove = tags.Select(t => (Tag)t);
        var albumTags = await albumService.RemoveTags(albumId, tagsToRemove);

        if (albumTags.Value is null)
        {
            string message = $"Failed to remove {tags.Count()} tags from {nameof(Album)} #{albumId}.";
            logging
                .Action(nameof(AlbumHandler.RemoveTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return albumTags.Result!;
        }

        return albumTags.Value
            .Select(t => (TagDTO)t)
            .ToArray();
    }

    /// <summary>
    /// Deletes the <see cref="Album"/> identified by <paramref name="albumId"/>
    /// </summary>
    public async Task<ActionResult> DeleteAlbum(int albumId)
    {
        if (albumId <= 0)
        {
            string message = $"Parameter {nameof(albumId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(AlbumHandler.DeleteAlbum))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var deleteAlbum = await albumService.DeleteAlbum(albumId);

        if (deleteAlbum is not NoContentResult)
        {
            logging
                .Action(nameof(AlbumHandler.DeleteAlbum))
                .ExternalDebug($"Failed to find an {nameof(Album)} with ID #{albumId}.")
                .LogAndEnqueue();
        }

        return deleteAlbum;
    }
}
