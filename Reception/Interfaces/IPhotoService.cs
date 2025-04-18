
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Reception.Models;

namespace Reception.Interfaces;

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
    public abstract Task<ActionResult<PhotoEntity>> GetPhotoEntity(int photoId);

    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> GetPhotoEntity(string slug);


    /// <summary>
    /// Get the <see cref="Reception.Models.Photo"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public abstract Task<ActionResult<Photo>> GetSinglePhoto(int photoId, Dimension dimension = Dimension.SOURCE);

    /// <summary>
    /// Get the <see cref="Reception.Models.Photo"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<Photo>> GetSinglePhoto(string slug, Dimension dimension = Dimension.SOURCE);


    /// <summary>
    /// Get the <see cref="PhotoCollection"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public abstract Task<ActionResult<PhotoCollection>> GetPhoto(int photoId);

    /// <summary>
    /// Get the <see cref="PhotoCollection"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public abstract Task<ActionResult<PhotoCollection>> GetPhoto(string slug);
    #endregion


    #region Get many photos.
    /// <summary>
    /// Get all <see cref="Reception.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Photo>>> GetSingles(string? search, Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetSingles(search, filtering);
    }

    /// <summary>
    /// Get all <see cref="Reception.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Photo>>> GetSingles(string? search, FilterPhotosOptions filter);


    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(string? search, Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(search, filtering);
    }

    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(string? search, FilterPhotosOptions filter);
    #endregion


    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public virtual Task<ActionResult<PhotoEntity>> CreatePhotoEntity(MutatePhoto mut) =>
        CreatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> CreatePhotoEntity(PhotoEntity entity);
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.PhotoEntity"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(MutatePhoto mut);


    /// <summary>
    /// Removes a <see cref="Reception.Models.Entities.Tag"/> (..identified by <paramref name="tag"/>) from the
    /// <see cref="Reception.Models.Entities.PhotoEntity"/> identified by its PK <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult> RemoveTag(int photoId, string tag);
    #endregion


    #region Delete a photo completely (blob, filepaths & photo)
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public abstract Task<ActionResult> DeletePhoto(int photoId);
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public abstract Task<ActionResult> DeletePhoto(PhotoEntity entity);
    #endregion


    #region Delete a blob from disk
    /// <summary>
    /// Deletes the blob of a <see cref="Reception.Models.Entities.Photo"/> from disk.
    /// </summary>
    public abstract Task<ActionResult> DeletePhotoBlob(Filepath entity);
    #endregion


    #region Delete a photo entities from the database
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public abstract Task<ActionResult> DeletePhotoEntity(int photoId);

    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public virtual Task<ActionResult> DeletePhotoEntity(MutatePhoto mut) =>
        DeletePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public abstract Task<ActionResult> DeletePhotoEntity(PhotoEntity entity);
    #endregion
}
