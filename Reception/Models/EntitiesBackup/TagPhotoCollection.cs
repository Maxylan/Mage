/*
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Reception.Database.Models;
using Swashbuckle.AspNetCore.Annotations;
*/

// namespace Reception.Models;

/// <summary>
/// Collection of all photos (<see cref="Reception.Models.PhotoCollection"/>) tagged with the given <paramref name="tag"/>.
/// </summary>
/*
public record TagPhotoCollection
{
    private Tag _tag;
    private Lazy<IEnumerable<PhotoCollection>> _collection;

    [SwaggerIgnore]
    public int Id { get => _tag.Id; }
    public string Name { get => _tag.Name; }
    public string? Description { get => _tag.Description; }

    /// <summary>
    /// Returns the number of elements in a sequence. (See - <seealso cref="IEnumerable{PhotoCollection}.Count()"/>)
    /// </summary>
    public int Count => this._collection.Value.Count();

    public IEnumerable<PhotoCollection> Photos { get => _collection.Value; }

    [SetsRequiredMembers]
    public TagPhotoCollection(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
        ArgumentNullException.ThrowIfNull(tag.Photos, nameof(tag.Photos));

        _tag = tag;
        _collection = new(
            () => _tag.Photos
                .Where(p => p.Filepaths is not null)
                .Select(p => new PhotoCollection(p))
        );
    }

    public PhotoCollection this[int index] {
        get => this._collection.Value.ElementAt(index);
    }
}
*/
