using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Reception.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Models;

/// <summary>
/// Collection of all photos (<see cref="Reception.Models.PhotoCollection"/>) tagged with the given <paramref name="tag"/>.
/// </summary>
public record TagAlbumCollection
{
    private Tag _tag;
    private ICollection<Album> _collection;

    [SwaggerIgnore]
    public int Id { get => _tag.Id; }
    public string Name { get => _tag.Name; }
    public string? Description { get => _tag.Description; }
    public IEnumerable<Album> Photos { get => _collection; }

    [SetsRequiredMembers]
    public TagAlbumCollection(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
        ArgumentNullException.ThrowIfNull(tag.Photos, nameof(tag.Photos));

        _tag = tag;
        _collection = tag.Albums;
    }

    public Album this[int index] {
        get => this._collection.ElementAt(index);
    }

    /// <summary>
    /// Returns the number of elements in a sequence. (See - <seealso cref="ICollection{PhotoCollection}.Count"/>)
    /// </summary>
    public int Count => this._collection.Count;
}
