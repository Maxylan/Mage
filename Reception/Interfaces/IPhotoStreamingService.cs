using Microsoft.AspNetCore.Mvc;
using Reception.Models;

namespace Reception.Interfaces;

public interface IPhotoStreamingService
{
    #region Create / Store photos.
    /// <summary>
    /// Upload any amount of new photos/files (<see cref="Photo"/>, <seealso cref="Reception.Database.Models.DisplayPhoto"/>)
    /// by streaming them directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as options/details of the
    /// generated database entitities.
    /// </remarks>
    /// <returns><see cref="DisplayPhoto"/></returns>
    public virtual Task<ActionResult<IEnumerable<DisplayPhoto>>> UploadPhotos(Action<PhotosOptions> opts)
    {
        FilterPhotosOptions options = new();
        opts(options);

        return UploadPhotos(options);
    }

    /// <summary>
    /// Upload any amount of new photos/files (<see cref="Photo"/>, <seealso cref="Reception.Database.Models.DisplayPhoto"/>)
    /// by streaming them directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="options"/>) has been repurposed to serve as options/details of the
    /// generated database entitities.
    /// </remarks>
    /// <returns><see cref="DisplayPhoto"/></returns>
    public abstract Task<ActionResult<IEnumerable<DisplayPhoto>>> UploadPhotos(PhotosOptions options);
    #endregion
}
