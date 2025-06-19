using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="AlbumTagRelation"/> data transfer object (DTO).
/// </summary>
public class PhotoAlbumRelationDTO : PhotoAlbumRelation, IDataTransferObject<PhotoAlbumRelation>
{
    /*
    [JsonPropertyName("photo_id")]
    public new int PhotoId { get; set; }

    [JsonPropertyName("album_id")]
    public new int AlbumId { get; set; }

    [JsonPropertyName("added")]
    public new DateTime Added { get; set; }
    */

    [JsonIgnore, SwaggerIgnore]
    public new Album Album { get; set; } = null!;

    [JsonIgnore, SwaggerIgnore]
    public new Photo Photo { get; set; } = null!;

    /// <summary>
    /// Convert this <see cref="PhotoAlbumRelationDTO"/> instance to its <see cref="PhotoAlbumRelation"/> equivalent.
    /// </summary>
    public PhotoAlbumRelation ToEntity() => new() {
        PhotoId = this.PhotoId,
        AlbumId  = this.AlbumId,
        Added  = this.Added,
        // Navigations
        Album = this.Album,
        Photo = this.Photo
    };

    /// <summary>
    /// Compare this <see cref="PhotoAlbumRelationDTO"/> against its <see cref="PhotoAlbumRelation"/> equivalent.
    /// </summary>
    public bool Equals(PhotoAlbumRelation entity) {
        throw new NotImplementedException();
    }
}
