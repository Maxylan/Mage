using Microsoft.AspNetCore.Mvc;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Interfaces.DataAccess;

public interface IPhotoService
{
    #region Get base filepaths.
    /// <summary>
    /// Get the name (only) of the base directory of my file storage
    /// </summary>
    public abstract string GetBaseDirectoryName();
    /// <summary>
    /// Get the name (only) of the Thumbnail directory of my file storage
    /// </summary>
    public abstract string GetThumbnailDirectoryName();
    /// <summary>
    /// Get the name (only) of the Medium directory of my file storage
    /// </summary>
    public abstract string GetMediumDirectoryName();
    /// <summary>
    /// Get the name (only) of the Source directory of my file storage
    /// </summary>
    public abstract string GetSourceDirectoryName();
    /// <summary>
    /// Get the path (directories, plural) to the directory relative to a <see cref="DateTime"/>
    /// </summary>
    public abstract string GetDatePath(DateTime dateTime);
    /// <summary>
    /// Get the <strong>combined</strong> relative path (<c>Base + Thumbnail/Medium/Source + DatePath</c>) to a directory in my file storage.
    /// </summary>
    public abstract string GetCombinedPath(Dimension dimension, DateTime? dateTime = null, string filename = "");
    #endregion


    #region Get single photos.
    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public abstract Task<ActionResult<Photo>> GetPhotoEntity(int photoId);

    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<Photo>> GetPhotoEntity(string slug);


    /// <summary>
    /// Get the <see cref="DisplayPhoto"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public abstract Task<ActionResult<DisplayPhoto>> GetPhoto(int photoId);

    /// <summary>
    /// Get the <see cref="DisplayPhoto"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<DisplayPhoto>> GetPhoto(string slug);
    #endregion


    #region Get many photos.
    /// <summary>
    /// Get all <see cref="Reception.Database.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Photo>>> GetPhotoEntities(string? search, Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotoEntities(search, filtering);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{Reception.Database.Models.Photo}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Photo>>> GetPhotoEntities(string? search, FilterPhotosOptions filter);


    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.DisplayPhoto}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(string? search, Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(search, filtering);
    }

    /// <summary>
    /// Assemble a <see cref="IEnumerable{Reception.Models.DisplayPhoto}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(string? search, FilterPhotosOptions filter);
    #endregion


    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Photo>> CreatePhotoEntity(MutatePhoto mut);
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Updates a <see cref="Reception.Database.Models.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Photo>> UpdatePhotoEntity(MutatePhoto mut);


    /// <summary>
    /// Adds the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) to the
    /// <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> AddTags(int photoId, IEnumerable<Tag> tag);


    /// <summary>
    /// Removes the given <see cref="IEnumerable{Reception.Database.Models.Tag}"/> collection (<paramref name="tags"/>) from
    /// the <see cref="Reception.Database.Models.Photo"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> RemoveTags(int photoId, IEnumerable<Tag> tags);
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
    protected abstract Task<ActionResult> DeletePhotoBlob(Filepath entity);
    #endregion


    #region Delete a photo entities from the database
    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..and associated <see cref="Reception.Database.Models.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    protected abstract Task<ActionResult> DeletePhotoEntity(int photoId);

    /// <summary>
    /// Deletes a <see cref="Reception.Database.Models.Photo"/> (..and associated <see cref="Reception.Database.Models.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    protected abstract Task<ActionResult> DeletePhotoEntity(Photo entity);
    #endregion
}
