
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


    #region Create a Filepath entity.
    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Filepath"/> in the database.
    /// </summary>
    /// <remarks>
    /// <trong>Note:</strong> Assumes a size of <see cref="Reception.Models.Entities.Dimension.SOURCE"/>.
    /// </remarks>
    public virtual Task<ActionResult<Filepath>> CreateFilepathEntity(string filename, int photoId) =>
        CreateFilepathEntity(Dimension.SOURCE, filename, photoId);

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public virtual Task<ActionResult<Filepath>> CreateFilepathEntity(Dimension dimension, string filename, int photoId)
    {
        if (string.IsNullOrWhiteSpace(filename)) {
            throw new NotImplementedException("Filename null or empty"); // TODO: HANDLE
        }

        return CreateFilepathEntity(new Filepath() {
            Dimension = dimension,
            Filename = filename,
            PhotoId = photoId
        });
    }
        
    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Filepath"/> in the database.
    /// </summary>
    /// <remarks>
    /// <trong>Note:</strong> Assumes a size of <see cref="Reception.Models.Entities.Dimension.SOURCE"/>.
    /// </remarks>
    public virtual async Task<ActionResult<Filepath[]>> CreateFilepathEntity(PhotoEntity photo, string? filename)
    {
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(photo.Filepaths));
        List<Filepath> paths = [];

        foreach(Filepath path in photo.Filepaths)
        {
            if (!string.IsNullOrWhiteSpace(filename)) {
                path.Filename = filename;
            }
            else if (string.IsNullOrWhiteSpace(path.Filename)) {
                continue; // Skip `path` if it has no Filename
            }

            if (path.PhotoId <= 0 || path.PhotoId != photo.Id) {
                path.PhotoId = photo.Id;
            }

            path.Photo ??= photo;

            var createFilepath = await CreateFilepathEntity(path);
            if (createFilepath.Value is null) {
                return createFilepath.Result!;
            }

            paths.Add(createFilepath.Value);
        }

        return paths.ToArray();
    }

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Filepath"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Filepath>> CreateFilepathEntity(Filepath path);
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
