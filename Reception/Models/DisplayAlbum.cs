using System.Collections;
using Swashbuckle.AspNetCore.Annotations;
using Reception.Database.Models;

namespace Reception.Models;

/// <summary>
/// Collection of all photos (<see cref="Reception.Models.DisplayPhoto"/>) inside the the given
/// <paramref name="album"/> (<see cref="Reception.Database.Models.AlbumDTO"/>).
/// </summary>
public record class DisplayAlbum : IEnumerable<DisplayPhoto>
{
    protected readonly IEnumerable<DisplayPhoto> _collection;

    public PhotoCollection this[int index] {
        get => this._collection.ElementAt(index);
    }

    public DisplayAlbum(AlbumDTO album)
    {
        ArgumentNullException.ThrowIfNull(album, nameof(album));
        ArgumentNullException.ThrowIfNull(album.Photos, nameof(album.Photos));

        _album = album;
        _collection = _album.Photos
                .Where(p => p.Filepaths is not null && p.Filepaths.Count > 0)
                .Select(p => new DisplayPhoto(p));
    }

    public IEnumerable<PhotoCollection> Photos { get => _collection; }

    /// <summary>
    /// Returns the number of elements (photos) in this sequence. (See - <seealso cref="IEnumerable{PhotoCollection}.Count()"/>)
    /// </summary>
    public int Count => this._collection.Count();

    public readonly int AlbumId;
    public readonly int? ThumbnailId;
    public readonly DisplayPhoto? Thumbnail;
    public readonly int? CategoryId;
    public readonly Category? Category;
    public readonly string Title;
    public readonly string? Summary;
    public readonly string? Description;
    public readonly TagDTO[] Tags;
    public readonly int? CreatedBy;
    public readonly DateTime CreatedAt;
    public readonly DateTime UpdatedAt;
    public readonly byte RequiredPrivilege;
}
