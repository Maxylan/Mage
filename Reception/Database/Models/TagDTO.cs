using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Tag"/> data transfer object (DTO).
/// </summary>
public class TagDTO : Tag, IDataTransferObject<Tag>, ITag
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("name")]
    public new string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public new string? Description { get; set; }

    [JsonPropertyName("required_privilege")]
    public new byte RequiredPrivilege { get; set; }
    */

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<AlbumTagRelation> UsedByAlbums { get; set; } = new List<AlbumTagRelation>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<PhotoTagRelation> UsedByPhotos { get; set; } = new List<PhotoTagRelation>();

    // public int Items => this.UsedByAlbums.Count + this.UsedByPhotos.Count;

    /// <summary>
    /// Convert this <see cref="TagDTO"/> instance to its <see cref="Tag"/> equivalent.
    /// </summary>
    public Tag ToEntity() => new() {
        Id = this.Id ?? default,
        Name = this.Name,
        Description = this.Description,
        RequiredPrivilege = this.RequiredPrivilege,
        // Navigations
        UsedByAlbums = this.UsedByAlbums,
        UsedByPhotos = this.UsedByPhotos
    };

    /// <summary>
    /// Compare this <see cref="TagDTO"/> against its <see cref="Tag"/> equivalent.
    /// </summary>
    public bool Equals(Tag entity) {
        throw new NotImplementedException();
    }
}
