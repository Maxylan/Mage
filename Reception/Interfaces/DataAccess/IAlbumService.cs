using Reception.Models;
using Reception.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces.DataAccess;

public interface IAlbumService
{
    /// <summary>
    /// Get the <see cref="Album"/> with Primary Key '<paramref ref="albumId"/>'
    /// </summary>
    public abstract Task<ActionResult<Album>> GetAlbum(int albumId);

    /// <summary>
    /// Get all <see cref="Album"/> instances matching a range of optional filtering / pagination options (<seealso cref="FilterAlbumsOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<Album>>> GetAlbums(Action<FilterAlbumsOptions> opts)
    {
        FilterAlbumsOptions filtering = new();
        opts(filtering);

        return GetAlbums(filtering);
    }

    /// <summary>
    /// Get all <see cref="Album"/> instances matching a range of optional filtering / pagination options (<seealso cref="FilterAlbumsOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Album>>> GetAlbums(FilterAlbumsOptions filter);

    /// <summary>
    /// Create a new <see cref="Reception.Models.Entities.Album"/>.
    /// </summary>
    public abstract Task<ActionResult<Album>> CreateAlbum(MutateAlbum mut);

    /// <summary>
    /// Updates an <see cref="Reception.Models.Entities.Album"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Album>> UpdateAlbum(MutateAlbum mut);

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (int[]).
    /// </summary>
    public abstract Task<ActionResult<Album>> MutateAlbumPhotos(int albumId, int[] photoIds);

    /// <summary>
    /// Removes a <see cref="Reception.Models.Entities.PhotoEntity"/> (..identified by PK <paramref name="photoId"/>) from the
    /// <see cref="Reception.Models.Entities.Album"/> identified by its PK <paramref name="albumId"/>.
    /// </summary>
    public abstract Task<ActionResult> RemovePhoto(int albumId, int photoId);

    /// <summary>
    /// Removes a <see cref="Reception.Models.Entities.Tag"/> (..identified by PK <paramref name="tagId"/>) from the
    /// <see cref="Reception.Models.Entities.Album"/> identified by its PK <paramref name="albumId"/>.
    /// </summary>
    public abstract Task<ActionResult> RemoveTag(int albumId, string tag);

    /// <summary>
    /// Deletes the <see cref="Reception.Models.Entities.Album"/> identified by <paramref name="albumId"/>
    /// </summary>
    public abstract Task<ActionResult> DeleteAlbum(int albumId);
}
