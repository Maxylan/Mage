using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Filepath"/> data transfer object (DTO).
/// </summary>
public class FilepathDTO : Filepath, IDataTransferObject<Filepath>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("photo_id")]
    public new int PhotoId { get; set; }

    [JsonPropertyName("filename")]
    public new string Filename { get; set; } = null!;

    [JsonPropertyName("path")]
    public new string Path { get; set; } = null!;

    [JsonPropertyName("filesize")]
    public new int? Filesize { get; set; }

    [JsonPropertyName("width")]
    public new int? Width { get; set; }

    [JsonPropertyName("height")]
    public new int? Height { get; set; }
    */

    /*
    [JsonIgnore, SwaggerIgnore]
    public new Photo Photo { get; set; } = null!;
    */

    /*
    public bool IsSource =>
        this.Dimension == Reception.Database.Dimension.SOURCE;

    public bool IsMedium =>
        this.Dimension == Reception.Database.Dimension.MEDIUM;

    public bool IsThumbnail =>
        this.Dimension == Reception.Database.Dimension.THUMBNAIL;
    */

    /// <summary>
    /// Convert this <see cref="FilepathDTO"/> instance to its <see cref="Filepath"/> equivalent.
    /// </summary>
    public Filepath ToEntity() => new() {
        Id = this.Id ?? default,
        PhotoId = this.PhotoId,
        Filename = this.Filename,
        Path = this.Path,
        Dimension = this.Dimension,
        Filesize = this.Filesize,
        Width = this.Width,
        Height = this.Height,
        // Navigations
        Photo = this.Photo
    };

    /// <summary>
    /// Compare this <see cref="FilepathDTO"/> against its <see cref="Filepath"/> equivalent.
    /// </summary>
    public bool Equals(Filepath entity) {
        throw new NotImplementedException();
    }
}
