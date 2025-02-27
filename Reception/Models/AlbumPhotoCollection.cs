using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Reception.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Models;

/// <summary>
/// Collection of all photos (<see cref="Reception.Models.PhotoCollection"/>) inside the the given <paramref name="album"/>.
/// </summary>
public record AlbumPhotoCollection
{
    private Album _album;
    private Lazy<IEnumerable<PhotoCollection>> _collection;

    [SwaggerIgnore]
    public int AlbumId { get => _album.Id; }
    public int? CategoryId { get => _album.CategoryId; }
    public string? CategoryTitle { get => _album.Category?.Title; }
    public int? ThumbnailId { get => _album.ThumbnailId; }
    public string? ThumbnailTitle { get => _album.Thumbnail?.Title; }
    public Photo? Thumbnail {
        get => _album.Thumbnail is not null
            ? new Photo(_album.Thumbnail, Dimension.THUMBNAIL, true)
            : null;
    }
    public string Title { get => _album.Title; }
    public string? Summary { get => _album.Summary; }
    public string? Description { get => _album.Description; }
    public string[] Tags { get => _album.Tags; }
    public int? CreatedBy { get => _album.CreatedBy; }
    public DateTime CreatedAt { get => _album.CreatedAt; }
    public DateTime UpdatedAt { get => _album.UpdatedAt; }
    public IEnumerable<PhotoCollection> Photos { get => _collection.Value; }

    [SetsRequiredMembers]
    public AlbumPhotoCollection(Album album)
    {
        ArgumentNullException.ThrowIfNull(album, nameof(album));
        ArgumentNullException.ThrowIfNull(album.Photos, nameof(album.Photos));

        _album = album;
        _collection = new(
            () => _album.Photos
                .Where(p => p.Filepaths is not null)
                .Select(p => new PhotoCollection(p))
        );
    }

    public PhotoCollection this[int index] {
        get => this._collection.Value.ElementAt(index);
    }

    /// <summary>
    /// Returns the number of elements in a sequence. (See - <seealso cref="IEnumerable{PhotoCollection}.Count()"/>)
    /// </summary>
    public int Count => this._collection.Value.Count();
}
