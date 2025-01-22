
using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using PhotoEntity = Reception.Models.Entities.Photo;
using Photo = Reception.Models.Photo;
using Reception.Models;

namespace Reception.Interfaces;

public interface IPhotoService
{
    #region Get single photos.
    /// <summary>
    /// Get the <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> GetPhotoEntity(int photoId);

    /// <summary>
    /// Get the <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) with Slug '<paramref ref="slug"/>' (string)
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
    public virtual Task<ActionResult<IEnumerable<Photo>>> GetSingles(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetSingles(filtering);
    }

    /// <summary>
    /// Get all <see cref="Reception.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Photo>>> GetSingles(FilterPhotosOptions filter);


    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional 
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(filtering);
    }

    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional 
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(FilterPhotosOptions filter);
    #endregion


    #region Create / Store a photo blob.
    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="FilterPhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public virtual Task<ActionResult<PhotoCollection>> UploadPhoto(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return UploadPhoto(filtering);
    }

    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="FilterPhotosOptions"/> (<paramref name="details"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public abstract Task<ActionResult<PhotoCollection>> UploadPhoto(FilterPhotosOptions details);
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
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..in the database.
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(int photoId);

    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public virtual Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(MutatePhoto mut) =>
        UpdatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(PhotoEntity entity);
    #endregion


    #region Delete a photo completely (blob, filepaths & photo)
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> DeletePhoto(int photoId);
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> DeletePhoto(PhotoEntity entity);
    #endregion


    #region Delete a blob from disk
    /// <summary>
    /// Deletes the blob of a <see cref="Reception.Models.Entities.Photo"/> from disk.
    /// </summary>
    public abstract Task<ActionResult<PhotoEntity>> DeletePhotoBlob(Filepath entity);
    #endregion


    #region Delete a photo entities from the database
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public virtual Task<ActionResult<PhotoEntity>> DeletePhotoEntity(MutatePhoto mut) =>
        UpdatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public abstract Task<ActionResult<PhotoEntity>> DeletePhotoEntity(PhotoEntity entity);
    #endregion
}