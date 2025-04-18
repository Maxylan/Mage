using Reception.Models;
using Reception.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface IBlobService
{
    /// <summary>
    /// Get the source blob associated with the <see cref="PhotoEntity"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public abstract Task<ActionResult> GetSourceBlobBySlug(string slug);
    /// <summary>
    /// Get the source blob associated with the <see cref="PhotoEntity"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult> GetSourceBlob(int photoId);
    /// <summary>
    /// Get the source blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public virtual Task<ActionResult> GetSourceBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);

        if (photo.Dimension != Dimension.SOURCE) {
            throw new ArgumentException($"Incorrect dimension ('{photo.Dimension.ToString()}') in given photo '{photo.Slug}' (#{photo.PhotoId})! Expected dimension '{Dimension.SOURCE.ToString()}'");
        }

        return GetBlob(photo);
    }

    /// <summary>
    /// Get the medium blob associated with the <see cref="PhotoEntity"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public abstract Task<ActionResult> GetMediumBlobBySlug(string slug);
    /// <summary>
    /// Get the medium blob associated with the <see cref="PhotoEntity"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult> GetMediumBlob(int photoId);
    /// <summary>
    /// Get the medium blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public virtual Task<ActionResult> GetMediumBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);

        if (photo.Dimension != Dimension.MEDIUM) {
            throw new ArgumentException($"Incorrect dimension ('{photo.Dimension.ToString()}') in given photo '{photo.Slug}' (#{photo.PhotoId})! Expected dimension '{Dimension.MEDIUM.ToString()}'");
        }

        return GetBlob(photo);
    }

    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="PhotoEntity"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public abstract Task<ActionResult> GetThumbnailBlobBySlug(string slug);
    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="PhotoEntity"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult> GetThumbnailBlob(int photoId);
    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public virtual Task<ActionResult> GetThumbnailBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);
        if (photo.Dimension != Dimension.THUMBNAIL) {
            throw new ArgumentException($"Incorrect dimension ('{photo.Dimension.ToString()}') in given photo '{photo.Slug}' (#{photo.PhotoId})! Expected dimension '{Dimension.THUMBNAIL.ToString()}'");
        }

        return GetBlob(photo);
    }


    /// <summary>
    /// Get the blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public Task<ActionResult> GetBlob(Dimension dimension, PhotoEntity entity) =>
        this.GetBlob(new Photo(entity, dimension));

    /// <summary>
    /// Get the blob associated with the <see cref="PhotoEntity"/> <paramref name="photo"/>
    /// </summary>
    public abstract Task<ActionResult> GetBlob(Photo photo);
}
