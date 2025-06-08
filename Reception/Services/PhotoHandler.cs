using System.Net;
using Microsoft.AspNetCore.Mvc;
using Reception.Services.DataAccess;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Services;

public class PhotoHandler(
    ILoggingService<PhotoService> logging,
    IPhotoService photoService
) : IPhotoHandler
{
    #region Get single photos.
    /// <summary>
    /// Get the <see cref="Photo"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<PhotoDTO>> GetPhoto(int photoId)
    {
        if (photoId <= 0)
        {
            string message = $"Parameter {nameof(photoId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(PhotoHandler.GetPhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getPhoto = await photoService.GetPhoto(photoId);

        if (getPhoto.Value is null)
        {
            string message = $"Failed to get a {nameof(Photo)} with ID #{photoId}.";
            logging
                .Action(nameof(PhotoHandler.GetPhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getPhoto.Result!;
        }

        return (PhotoDTO)getPhoto.Value;
    }

    /// <summary>
    /// Get the <see cref="Photo"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<PhotoDTO>> GetPhoto(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            string message = $"Parameter {nameof(slug)} cannot be null/omitted!";
            logging
                .Action(nameof(PhotoHandler.GetPhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getPhoto = await photoService.GetPhoto(slug);

        if (getPhoto.Value is null)
        {
            string message = $"Failed to get a {nameof(Photo)} with slug '{slug}'.";
            logging
                .Action(nameof(PhotoHandler.GetPhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getPhoto.Result!;
        }

        return (PhotoDTO)getPhoto.Value;
    }

    /// <summary>
    /// Get the <see cref="DisplayPhoto"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<DisplayPhoto>> GetDisplayPhoto(int photoId)
    {
        var getPhoto = await GetPhoto(photoId);

        if (getPhoto.Value is null)
        {
            /*
            // No need to double-log..
            string message = $"Failed to get a {nameof(Photo)} with ID #{photoId}.";
            logging
                .Action(nameof(PhotoHandler.GetPhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();
            */
            return getPhoto.Result!;
        }

        return new DisplayPhoto(getPhoto.Value);
    }

    /// <summary>
    /// Get the <see cref="DisplayPhoto"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<DisplayPhoto>> GetDisplayPhoto(string slug)
    {
        var getPhoto = await photoService.GetPhoto(slug);

        if (getPhoto.Value is null)
        {
            /*
            // No need to double-log..
            string message = $"Failed to get a {nameof(Photo)} with slug '{slug}'.";
            logging
                .Action(nameof(PhotoHandler.GetPhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();
            */
            return getPhoto.Result!;
        }

        return new DisplayPhoto(getPhoto.Value);
    }
    #endregion


    #region Get many photos.
    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<PhotoDTO>>> GetPhotos(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(filtering);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{Reception.Database.Models.Photo}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<PhotoDTO>>> GetPhotos(FilterPhotosOptions filter)
    {
        if (filter.Limit is not null && filter.Limit <= 0)
        {
            string message = $"Parameter {nameof(filter.Limit)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(PhotoHandler.GetPhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (filter.Offset is not null && filter.Offset < 0)
        {
            string message = $"Parameter {nameof(filter.Offset)} has to be a positive integer!";
            logging
                .Action(nameof(PhotoHandler.GetPhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getPhotos = await photoService.GetPhotos(filter);

        if (getPhotos.Value is null)
        {
            string message = $"Failed to get any {nameof(Photo)}(s) matching the given filter!";
            logging
                .Action(nameof(PhotoHandler.GetPhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getPhotos.Result!;
        }

        return getPhotos.Value
            .Select(photo => (PhotoDTO)photo)
            .ToArray();
    }


    /// <summary>
    /// Get all <see cref="DisplayPhoto"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<DisplayPhoto>>> GetDisplayPhotos(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetDisplayPhotos(filtering);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{DisplayPhoto}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<DisplayPhoto>>> GetDisplayPhotos(FilterPhotosOptions filter)
    {
        var getPhotos = await GetPhotos(filter);

        if (getPhotos.Value is null)
        {
            /*
            // No need to double-log..
            string message = $"Failed to get any {nameof(Photo)}(s) matching the given filter!";
            logging
                .Action(nameof(PhotoHandler.GetDisplayPhotos))
                .ExternalDebug(message)
                .LogAndEnqueue();
            */
            return getPhotos.Result!;
        }

        return getPhotos.Value
            .Select(photo => new DisplayPhoto(photo))
            .ToArray();
    }


    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances by evaluating a wide range of optional search / pagination options (<seealso cref="PhotoSearchQuery"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<PhotoDTO>>> PhotoSearch(string searchTerm, Action<PhotoSearchQuery> opts)
    {
        PhotoSearchQuery search = new();
        opts(search);

        return PhotoSearch(searchTerm, search);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{Reception.Database.Models.Photo}"/> collection of Photos by evaluating a wide range of optional
    /// search / pagination options (<seealso cref="PhotoSearchQuery"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<PhotoDTO>>> PhotoSearch(string searchTerm, PhotoSearchQuery searchQuery)
    {
        if (searchQuery.Limit is not null && searchQuery.Limit <= 0)
        {
            string message = $"Parameter {nameof(searchQuery.Limit)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(PhotoHandler.PhotoSearch))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (searchQuery.Offset is not null && searchQuery.Offset < 0)
        {
            string message = $"Parameter {nameof(searchQuery.Offset)} has to be a positive integer!";
            logging
                .Action(nameof(PhotoHandler.PhotoSearch))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var searchPhoto = await photoService.PhotoSearch(searchQuery);

        if (searchPhoto.Value is null)
        {
            string message = $"Failed to get any {nameof(Photo)}(s) matching the given search!";
            logging
                .Action(nameof(PhotoHandler.PhotoSearch))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return searchPhoto.Result!;
        }

        return searchPhoto.Value
            .Select(photo => (PhotoDTO)photo)
            .ToArray();
    }


    /// <summary>
    /// Get all <see cref="DisplayPhoto"/> instances by evaluating a wide range of optional search / pagination options (<seealso cref="PhotoSearchQuery"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<DisplayPhoto>>> DisplayPhotosSearch(string searchTerm, Action<PhotoSearchQuery> opts)
    {
        PhotoSearchQuery search = new();
        opts(search);

        return DisplayPhotosSearch(searchTerm, search);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{DisplayPhoto}"/> collection of Photos by evaluating a wide range of optional
    /// search / pagination options (<seealso cref="PhotoSearchQuery"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<DisplayPhoto>>> DisplayPhotosSearch(string searchTerm, PhotoSearchQuery searchQuery)
    {
        var searchPhoto = await PhotoSearch(searchTerm, searchQuery);

        if (searchPhoto.Value is null)
        {
            /*
            // No need to double-log..
            string message = $"Failed to get any {nameof(Photo)}(s) matching the given search!";
            logging
                .Action(nameof(PhotoHandler.DisplayPhotosSearch))
                .ExternalDebug(message)
                .LogAndEnqueue();
            */
            return searchPhoto.Result!;
        }

        return searchPhoto.Value
            .Select(photo => new DisplayPhoto(photo))
            .ToArray();
    }
    #endregion


    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoDTO>> CreatePhoto(MutatePhoto mut)
    {
        var newPhoto = await photoService.CreatePhoto(mut);

        if (newPhoto.Value is null)
        {
            string message = $"Failed to create {nameof(Photo)} '{mut.Title}' ('{mut.Slug}')!";
            logging
                .Action(nameof(PhotoHandler.CreatePhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return newPhoto.Result!;
        }

        return (PhotoDTO)newPhoto.Value;
    }
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Updates a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoDTO>> UpdatePhoto(MutatePhoto mut)
    {
        if (mut.Id <= 0)
        {
            string message = $"Parameter {nameof(mut.Id)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(PhotoHandler.UpdatePhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (string.IsNullOrWhiteSpace(mut.Slug))
        {
            string message = $"Parameter {nameof(mut.Slug)} cannot be null/omitted!";
            logging
                .Action(nameof(PhotoHandler.UpdatePhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getPhoto = await photoService.UpdatePhoto(mut);

        if (getPhoto.Value is null)
        {
            string message = $"Failed to update {nameof(Photo)} #{mut.Id} ('{mut.Slug}').";
            logging
                .Action(nameof(PhotoHandler.UpdatePhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getPhoto.Result!;
        }

        return (PhotoDTO)getPhoto.Value;
    }


    /// <summary>
    /// Adds the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) to the
    /// <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> AddTags(int photoId, IEnumerable<ITag> tags)
    {
        if (photoId <= 0)
        {
            string message = $"Parameter {nameof(photoId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(PhotoHandler.AddTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var photoTags = await photoService.AddTags(photoId, tags);

        if (photoTags.Value is null)
        {
            string message = $"Failed to add {nameof(Tag)}(s) to {nameof(Photo)} #{photoId}.";
            logging
                .Action(nameof(PhotoHandler.AddTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return photoTags.Result!;
        }

        return photoTags.Value
            .Select(tag => (TagDTO)tag)
            .ToArray();
    }

    /// <summary>
    /// Removes the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) from
    /// the <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public async Task<ActionResult<IEnumerable<TagDTO>>> RemoveTags(int photoId, IEnumerable<ITag> tags)
    {
        if (photoId <= 0)
        {
            string message = $"Parameter {nameof(photoId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(PhotoHandler.RemoveTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var photoTags = await photoService.RemoveTags(photoId, tags);

        if (photoTags.Value is null)
        {
            string message = $"Failed to remove {nameof(Tag)}(s) from {nameof(Photo)} #{photoId}.";
            logging
                .Action(nameof(PhotoHandler.RemoveTags))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return photoTags.Result!;
        }

        return photoTags.Value
            .Select(tag => (TagDTO)tag)
            .ToArray();
    }
    #endregion


    #region Delete a photo completely (blob, filepaths & photo)
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        if (photoId <= 0)
        {
            string message = $"Parameter {nameof(photoId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(PhotoHandler.DeletePhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var deletePhotoResult = await photoService.DeletePhoto(photoId);

        if (deletePhotoResult is not OkResult &&
            deletePhotoResult is not OkObjectResult &&
            deletePhotoResult is not NoContentResult
        ) {
            string message = $"Failed to get a {nameof(Photo)} with ID #{photoId}.";
            logging
                .Action(nameof(PhotoHandler.DeletePhoto))
                .ExternalDebug(message)
                .LogAndEnqueue();
        }

        return deletePhotoResult;
    }
    #endregion
}
