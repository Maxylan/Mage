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
    /// Create a new <see cref="Reception.Database.Models.Album"/>.
    /// </summary>
    public abstract Task<ActionResult<Album>> CreateAlbum(MutateAlbum mut);

    /// <summary>
    /// Updates an <see cref="Reception.Database.Models.Album"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<Album>> UpdateAlbum(MutateAlbum mut);

    /// <summary>
    /// Add <see cref="Tag"/>(s) (<paramref name="tags"/>) ..to a <see cref="Album"/> identified by PK '<paramref ref="albumId"/>' (int)
    /// </summary>
    public abstract Task<ActionResult> ToggleFavorite(int albumId);

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (<see cref="IEnumerable{int}"/>).
    /// </summary>
    public abstract Task<ActionResult<Album>> AddPhotos(int albumId, IEnumerable<int> photoIds);

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (<see cref="IEnumerable{int}"/>).
    /// </summary>
    public abstract Task<ActionResult<Album>> RemovePhotos(int albumId, IEnumerable<int> photoIds);

    /// <summary>
    /// Update what tags are associated with this <see cref="Album"/> via <paramref name="tag"/> (<see cref="IEnumerable{Reception.Database.Models.Tag}"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> AddTags(int albumId, IEnumerable<Tag> tag);

    /// <summary>
    /// Update what tags are associated with this <see cref="Album"/> via <paramref name="tag"/> (<see cref="IEnumerable{Reception.Database.Models.Tag}"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<Tag>>> RemoveTags(int albumId, IEnumerable<Tag> tag);

    /// <summary>
    /// Deletes the <see cref="Reception.Database.Models.Album"/> identified by <paramref name="albumId"/>
    /// </summary>
    public abstract Task<ActionResult> DeleteAlbum(int albumId);
}
