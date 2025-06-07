using Reception.Database;
using Reception.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface IBlobService
{
    /// <summary>
    /// Get the source blob associated with the <see cref="Photo"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public abstract Task<ActionResult> GetSourceBlobBySlug(string slug);
    /// <summary>
    /// Get the source blob associated with the <see cref="Photo"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult> GetSourceBlob(int photoId);
    /// <summary>
    /// Get the source blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    public virtual Task<ActionResult> GetSourceBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(Photo.Filepaths));

        if (!photo.SourceExists) {
            throw new ArgumentException(
                $"Incorrect dimension(s) {nameof(Dimension)} in given photo '{photo.Slug}' (#{photo.Id})! Expected dimension '{Dimension.SOURCE.ToString()}'"
            );
        }

        return GetBlobAsync(photo, Dimension.SOURCE);
    }

    /// <summary>
    /// Get the medium blob associated with the <see cref="Photo"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public abstract Task<ActionResult> GetMediumBlobBySlug(string slug);
    /// <summary>
    /// Get the medium blob associated with the <see cref="Photo"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult> GetMediumBlob(int photoId);
    /// <summary>
    /// Get the medium blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    public virtual Task<ActionResult> GetMediumBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(Photo.Filepaths));

        if (!photo.MediumExists) {
            throw new ArgumentException(
                $"Incorrect dimension(s) {nameof(Dimension)} in given photo '{photo.Slug}' (#{photo.Id})! Expected dimension '{Dimension.MEDIUM.ToString()}'"
            );
        }

        return GetBlobAsync(photo, Dimension.MEDIUM);
    }

    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="Photo"/> identified by its unique <paramref name="slug"/>.
    /// </summary>
    public abstract Task<ActionResult> GetThumbnailBlobBySlug(string slug);
    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="Photo"/> identified by <paramref name="photoId"/>.
    /// </summary>
    public abstract Task<ActionResult> GetThumbnailBlob(int photoId);
    /// <summary>
    /// Get the thumbnail blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    public virtual Task<ActionResult> GetThumbnailBlob(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(Photo.Filepaths));

        if (!photo.ThumbnailExists) {
            throw new ArgumentException(
                $"Incorrect dimension(s) {nameof(Dimension)} in given photo '{photo.Slug}' (#{photo.Id})! Expected dimension '{Dimension.THUMBNAIL.ToString()}'"
            );
        }

        return GetBlobAsync(photo, Dimension.THUMBNAIL);
    }


    /// <summary>
    /// Get the blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public abstract ActionResult GetBlob(Photo entity, Dimension dimension);


    /// <summary>
    /// Asynchronously get the blob associated with the <see cref="Photo"/> (<paramref name="photo"/>)
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public virtual Task<ActionResult> GetBlobAsync(Photo entity, Dimension dimension) =>
        Task.Run(() => this.GetBlob(entity, dimension));
}
