using Reception.Models;
using Reception.Models.Entities;
using Reception.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Services;

public class AlbumService : IAlbumService
{
    /// <summary>
    /// Get the <see cref="Album"/> with Primary Key '<paramref ref="albumId"/>'
    /// </summary>
    public async Task<ActionResult<Album>> GetAlbum(int albumId)
    {
        throw new NotImplementedException();
    }

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
    public async Task<ActionResult<IEnumerable<Album>>> GetAlbums(FilterAlbumsOptions filter)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the <see cref="Album"/> with PK <paramref ref="albumId"/> (int), along with a collection of all associated Photos.
    /// </summary>
    public async Task<ActionResult<AlbumPhotoCollection>> GetAlbumPhotoCollection(int albumId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a new <see cref="Reception.Models.Entities.Album"/>.
    /// </summary>
    public async Task<ActionResult<Album>> CreateAlbum(MutateAlbum mut)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Album"/> in the database.
    /// </summary>
    public async Task<ActionResult<Album>> UpdateAlbum(MutateAlbum mut)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (int[]).
    /// </summary>
    public async Task<ActionResult<AlbumPhotoCollection>> MutateAlbumPhotos(int albumId, int[] photoIds)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes a <see cref="Reception.Models.Entities.PhotoEntity"/> (..identified by PK <paramref name="photoId"/>) from the
    /// <see cref="Reception.Models.Entities.Album"/> identified by its PK <paramref name="albumId"/>.
    /// </summary>
    public async Task<ActionResult> RemovePhoto(int albumId, int photoId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes a <see cref="Reception.Models.Entities.Tag"/> (..identified by PK <paramref name="tagId"/>) from the
    /// <see cref="Reception.Models.Entities.Album"/> identified by its PK <paramref name="albumId"/>.
    /// </summary>
    public async Task<ActionResult> RemoveTag(int albumId, string tag)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes the <see cref="Reception.Models.Entities.Album"/> identified by <paramref name="albumId"/>
    /// </summary>
    public async Task<ActionResult> DeleteAlbum(int albumId)
    {
        throw new NotImplementedException();
    }
}
