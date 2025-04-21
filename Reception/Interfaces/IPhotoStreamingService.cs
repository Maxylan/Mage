using Microsoft.AspNetCore.Mvc;
using Reception.Models.Entities;
using Reception.Models;

namespace Reception.Interfaces;

public interface IPhotoStreamingService
{
    #region Create / Store photos.
    /// <summary>
    /// Upload any amount of new photos/files (<see cref="PhotoEntity"/>, <seealso cref="Reception.Models.Entities.PhotoCollection"/>)
    /// by streaming them directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as options/details of the
    /// generated database entitities.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public virtual Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhotos(Action<PhotosOptions> opts)
    {
        FilterPhotosOptions options = new();
        opts(options);

        return UploadPhotos(options);
    }

    /// <summary>
    /// Upload any amount of new photos/files (<see cref="PhotoEntity"/>, <seealso cref="Reception.Models.Entities.PhotoCollection"/>)
    /// by streaming them directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="options"/>) has been repurposed to serve as options/details of the
    /// generated database entitities.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public abstract Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhotos(PhotosOptions options);
    #endregion
}
