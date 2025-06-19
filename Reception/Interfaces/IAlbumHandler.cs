using Reception.Models;
using Reception.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace Reception.Interfaces;

public interface IAlbumHandler
{
    /// <summary>
    /// Get the <see cref="Album"/> with Primary Key '<paramref ref="albumId"/>'
    /// </summary>
    public abstract Task<ActionResult<DisplayAlbum>> GetAlbum(int albumId);

    /// <summary>
    /// Get all <see cref="DisplayAlbum"/> instances matching a range of optional filtering / pagination options (<seealso cref="FilterAlbumsOptions"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<DisplayAlbum>>> FilterAlbums(Action<FilterAlbumsOptions> opts)
    {
        FilterAlbumsOptions filtering = new();
        opts(filtering);

        return FilterAlbums(filtering);
    }

    /// <summary>
    /// Get all <see cref="DisplayAlbum"/> instances matching a range of optional filtering / pagination options (<seealso cref="FilterAlbumsOptions"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<DisplayAlbum>>> FilterAlbums(FilterAlbumsOptions filter);

    /// <summary>
    /// Get all <see cref="DisplayAlbum"/> instances by evaluating a wide range of optional search / pagination options (<seealso cref="AlbumSearchQuery"/>).
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<DisplayAlbum>>> SearchForAlbums(Action<AlbumSearchQuery> opts)
    {
        AlbumSearchQuery filtering = new();
        opts(filtering);

        return SearchForAlbums(filtering);
    }

    /// <summary>
    /// Get all <see cref="DisplayAlbum"/> instances by evaluating a wide range of optional search / pagination options (<seealso cref="AlbumSearchQuery"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<DisplayAlbum>>> SearchForAlbums(AlbumSearchQuery filter);

    /// <summary>
    /// Create a new <see cref="Album"/>.
    /// </summary>
    public abstract Task<ActionResult<DisplayAlbum>> CreateAlbum(MutateAlbum mut);

    /// <summary>
    /// Updates an <see cref="Album"/> in the database.
    /// </summary>
    public abstract Task<ActionResult<DisplayAlbum>> UpdateAlbum(MutateAlbum mut);

    /// <summary>
    /// Add <see cref="Tag"/>(s) (<paramref name="tags"/>) ..to a <see cref="Album"/> identified by PK '<paramref ref="albumId"/>' (int)
    /// </summary>
    public abstract Task<ActionResult> ToggleFavorite(int albumId);

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (<see cref="IEnumerable{int}"/>).
    /// </summary>
    public abstract Task<ActionResult<DisplayAlbum>> AddPhotos(int albumId, IEnumerable<int> photoIds);

    /// <summary>
    /// Update what photos are associated with this <see cref="Album"/> via <paramref name="photoIds"/> (<see cref="IEnumerable{int}"/>).
    /// </summary>
    public abstract Task<ActionResult<DisplayAlbum>> RemovePhotos(int albumId, IEnumerable<int> photoIds);

    /// <summary>
    /// Update what tags are associated with this <see cref="Album"/> via <paramref name="tags"/> (<see cref="IEnumerable{Reception.Database.Models.ITag}"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> AddTags(int albumId, IEnumerable<ITag> tags);

    /// <summary>
    /// Update what tags are associated with this <see cref="Album"/> via <paramref name="tags"/> (<see cref="IEnumerable{Reception.Database.Models.ITag}"/>).
    /// </summary>
    public abstract Task<ActionResult<IEnumerable<TagDTO>>> RemoveTags(int albumId, IEnumerable<ITag> tags);

    /// <summary>
    /// Deletes the <see cref="Album"/> identified by <paramref name="albumId"/>
    /// </summary>
    public abstract Task<ActionResult> DeleteAlbum(int albumId);
}
