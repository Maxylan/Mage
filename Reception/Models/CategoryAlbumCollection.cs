using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Reception.Database.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Models;

/// <summary>
/// Collection of all albums (<see cref="Reception.Models.Entities.Album"/>) tagged with the given <paramref name="tag"/>.
/// </summary>
public record CategoryAlbumCollection
{
    private Category _category;
    private ICollection<Album> _collection;

    [SwaggerIgnore]
    public int Id { get => _category.Id; }
    public string Title { get => _category.Title; }
    public string? Summary { get => _category.Summary; }
    public string? Description { get => _category.Description; }
    public int? CreatedBy { get => _category.CreatedBy; }
    public DateTime CreatedAt { get => _category.CreatedAt; }
    public DateTime UpdatedAt { get => _category.UpdatedAt; }

    /// <summary>
    /// Returns the number of elements in a sequence. (See - <seealso cref="ICollection{Album}.Count"/>)
    /// </summary>
    public int Count => this._collection.Count;

    public IEnumerable<Album> Albums { get => _collection; }

    [SetsRequiredMembers]
    public CategoryAlbumCollection(Category category)
    {
        ArgumentNullException.ThrowIfNull(category, nameof(category));
        ArgumentNullException.ThrowIfNull(category.Albums, nameof(category.Albums));

        _category = category;
        _collection = category.Albums;
    }

    public Album this[int index] {
        get => this._collection.ElementAt(index);
    }
}
