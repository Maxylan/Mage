using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Interfaces.DataAccess;

public interface IPhotoService
{
    #region Get single photos.
    /// <summary>
    /// Get the <see cref="Photo"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public abstract Task<ActionResult<Photo>> GetPhoto(int photoId);

    /// <summary>
    /// Get the <see cref="Photo"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<Photo>> GetPhoto(string slug);
    #endregion


    #region Get many photos.
    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Photo>>> GetPhotos(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(filtering);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{Reception.Database.Models.Photo}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Photo>>> GetPhotos(FilterPhotosOptions filter);


    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances by evaluating a wide range of optional search / pagination options (<seealso cref="PhotoSearchQuery"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Photo>>> PhotoSearch(Action<PhotoSearchQuery> opts)
    {
        PhotoSearchQuery search = new();
        opts(search);

        return PhotoSearch(search);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{Reception.Database.Models.Photo}"/> collection of Photos by evaluating a wide range of optional
    /// search / pagination options (<seealso cref="PhotoSearchQuery"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Photo>>> PhotoSearch(PhotoSearchQuery searchQuery);
    #endregion


    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Photo>> CreatePhoto(MutatePhoto mut);

    /// <summary>
    /// Create a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Photo>> CreatePhoto(Photo entity);
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Toggles the 'Favorite' status of a <see cref="Reception.Database.Models.Photo"/> for a single user.
    /// </summary>
    public abstract Task<ActionResult> ToggleFavorite(int photoId);


    /// <summary>
    /// Updates a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Photo>> UpdatePhoto(MutatePhoto mut);


    /// <summary>
    /// Adds the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) to the
    /// <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> AddTags(int photoId, IEnumerable<ITag> tags);


    /// <summary>
    /// Removes the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) from
    /// the <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> RemoveTags(int photoId, IEnumerable<ITag> tags);
    #endregion


    #region Delete a photo completely (blob, filepaths & photo)
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public abstract Task<ActionResult> DeletePhoto(int photoId);
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..identified by PK <paramref name="entity"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public abstract Task<ActionResult> DeletePhoto(Photo entity);
    #endregion


    #region Delete a blob from disk
    /// <summary>
    /// Deletes the blob of a <see cref="Reception.Database.Models.Photo"/> from disk.
    /// </summary>
    public abstract Task<ActionResult> DeletePhotoBlob(Filepath entity);
    #endregion


    #region Delete a photo entities from the database
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..and associated <see cref="Reception.Database.Models.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public abstract Task<ActionResult> DeletePhotoEntity(int photoId);

    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..and associated <see cref="Reception.Database.Models.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public abstract Task<ActionResult> DeletePhotoEntity(Photo entity);
    #endregion
}
