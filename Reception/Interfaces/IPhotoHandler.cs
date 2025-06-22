using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces;

public interface IPhotoHandler
{
    #region Get single photos.
    /// <summary>
    /// Get the <see cref="Photo"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public abstract Task<ActionResult<PhotoDTO>> GetPhoto(int photoId);

    /// <summary>
    /// Get the <see cref="Photo"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<PhotoDTO>> GetPhoto(string slug);

    /// <summary>
    /// Get the <see cref="DisplayPhoto"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public abstract Task<ActionResult<DisplayPhoto>> GetDisplayPhoto(int photoId);

    /// <summary>
    /// Get the <see cref="DisplayPhoto"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<DisplayPhoto>> GetDisplayPhoto(string slug);
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
    public abstract Task<ActionResult<IEnumerable<PhotoDTO>>> GetPhotos(FilterPhotosOptions filter);


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
    public abstract Task<ActionResult<IEnumerable<DisplayPhoto>>> GetDisplayPhotos(FilterPhotosOptions filter);


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
    public abstract Task<ActionResult<IEnumerable<PhotoDTO>>> PhotoSearch(string searchTerm, PhotoSearchQuery searchQuery);


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
    public abstract Task<ActionResult<IEnumerable<DisplayPhoto>>> DisplayPhotosSearch(string searchTerm, PhotoSearchQuery searchQuery);
    #endregion


    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<PhotoDTO>> CreatePhoto(MutatePhoto mut);
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Toggles the 'Favorite' status of a <see cref="Reception.Database.Models.Photo"/> for a single user.
    /// </summary>
    public abstract Task<ActionResult> ToggleFavorite(int photoId);


    /// <summary>
    /// Updates a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<PhotoDTO>> UpdatePhoto(MutatePhoto mut);


    /// <summary>
    /// Adds the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) to the
    /// <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> AddTags(int photoId, IEnumerable<ITag> tags);


    /// <summary>
    /// Removes the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) from
    /// the <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> RemoveTags(int photoId, IEnumerable<ITag> tags);
    #endregion


    #region Delete a photo completely (blob, filepaths & photo)
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public abstract Task<ActionResult> DeletePhoto(int photoId);
    #endregion
}
