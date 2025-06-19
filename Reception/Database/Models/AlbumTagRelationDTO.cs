using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="AlbumTagRelation"/> data transfer object (DTO).
/// </summary>
public class AlbumTagRelationDTO : AlbumTagRelation, IDataTransferObject<AlbumTagRelation>
{
    [JsonPropertyName("album_id")]
    public new int AlbumId { get; set; }

    /*
    [JsonPropertyName("tag_id")]
    public new int TagId { get; set; }

    [JsonPropertyName("added")]
    public new DateTime Added { get; set; }
    */

    /*
    [JsonIgnore, SwaggerIgnore]
    public new Album Album { get; set; } = null!;

    [JsonIgnore, SwaggerIgnore]
    public new Tag Tag { get; set; } = null!;
    */

    /// <summary>
    /// Convert this <see cref="AlbumTagRelationDTO"/> instance to its <see cref="AlbumTagRelation"/> equivalent.
    /// </summary>
    public AlbumTagRelation ToEntity() => new() {
        AlbumId = this.AlbumId,
        TagId = this.TagId,
        Added = this.Added,
        // Navigations
        Album = this.Album,
        Tag = this.Tag
    };

    /// <summary>
    /// Compare this <see cref="AlbumTagRelationDTO"/> against its <see cref="AlbumTagRelation"/> equivalent.
    /// </summary>
    public bool Equals(AlbumTagRelation entity) {
        throw new NotImplementedException();
    }
}
