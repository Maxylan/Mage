using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="PhotoTagRelation"/> data transfer object (DTO).
/// </summary>
public class PhotoTagRelationDTO : PhotoTagRelation, IDataTransferObject<PhotoTagRelation>
{
    /*
    [JsonPropertyName("photo_id")]
    public new int PhotoId { get; set; }

    [JsonPropertyName("tag_id")]
    public new int TagId { get; set; }

    [JsonPropertyName("added")]
    public new DateTime Added { get; set; }
    */

    [JsonIgnore, SwaggerIgnore]
    public new Photo Photo { get; set; } = null!;

    [JsonIgnore, SwaggerIgnore]
    public new Tag Tag { get; set; } = null!;

    /// <summary>
    /// Convert this <see cref="PhotoTagRelationDTO"/> instance to its <see cref="PhotoTagRelation"/> equivalent.
    /// </summary>
    public PhotoTagRelation ToEntity() => new() {
        PhotoId = this.PhotoId,
        TagId  = this.TagId,
        Added  = this.Added,
        // Navigations
        Photo  = this.Photo,
        Tag  = this.Tag
    };

    /// <summary>
    /// Compare this <see cref="PhotoTagRelationDTO"/> against its <see cref="PhotoTagRelation"/> equivalent.
    /// </summary>
    public bool Equals(PhotoTagRelation entity) {
        throw new NotImplementedException();
    }
}
