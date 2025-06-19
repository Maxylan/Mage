using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Photo"/> data transfer object (DTO).
/// </summary>
public class PhotoDTO : Photo, IDataTransferObject<Photo>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("slug")]
    public new string Slug { get; set; } = null!;

    [JsonPropertyName("title")]
    public new string Title { get; set; } = null!;

    [JsonPropertyName("summary")]
    public new string? Summary { get; set; }

    [JsonPropertyName("description")]
    public new string? Description { get; set; }

    [JsonPropertyName("uploaded_by")]
    public new int? UploadedBy { get; set; }

    [JsonPropertyName("uploaded_at")]
    public new DateTime UploadedAt { get; set; }

    [JsonPropertyName("updated_by")]
    public new int? UpdatedBy { get; set; }

    [JsonPropertyName("updated_at")]
    public new DateTime UpdatedAt { get; set; }

    /// <summary>TypeName = "timestamp without time zone"</summary>
    [JsonPropertyName("created_at")]
    public new DateTime CreatedAt { get; set; }

    [JsonPropertyName("is_analyzed")]
    public new bool IsAnalyzed { get; set; }

    /// <summary>TypeName = "timestamp without time zone"</summary>
    [JsonPropertyName("analyzed_at")]
    public new DateTime? AnalyzedAt { get; set; }

    [JsonPropertyName("required_privilege")]
    public new byte RequiredPrivilege { get; set; }
    */

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Account> UsedAsAvatar { get; set; } = new List<Account>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Album> UsedAsThumbnail { get; set; } = new List<Album>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<FavoritePhotoRelation> FavoritedBy { get; set; } = new List<FavoritePhotoRelation>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Filepath> Filepaths { get; set; } = new List<Filepath>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<PublicLink> PublicLinks { get; set; } = new List<PublicLink>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<PhotoAlbumRelation> Albums { get; set; } = new List<PhotoAlbumRelation>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<PhotoTagRelation> Tags { get; set; } = new List<PhotoTagRelation>();

    [JsonIgnore, SwaggerIgnore]
    public new Account? UpdatedByNavigation { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new Account? UploadedByNavigation { get; set; }

    /*
    // Lil' helpers
    [SwaggerIgnore]
    public bool SourceExists =>
        this.Filepaths?.Any(path => path.Dimension == Dimension.SOURCE) == true;
    [SwaggerIgnore]
    public bool MediumExists =>
        this.Filepaths?.Any(path => path.Dimension == Dimension.MEDIUM) == true;
    [SwaggerIgnore]
    public bool ThumbnailExists =>
        this.Filepaths?.Any(path => path.Dimension == Dimension.THUMBNAIL) == true;
    */

    /// <summary>
    /// Convert this <see cref="PhotoDTO"/> instance to its <see cref="Photo"/> equivalent.
    /// </summary>
    public Photo ToEntity() => new() {
        Id = this.Id ?? default,
        Slug = this.Slug,
        Title  = this.Title,
        Summary  = this.Summary,
        Description = this.Description,
        UploadedBy  = this.UploadedBy,
        UploadedAt  = this.UploadedAt,
        UpdatedBy  = this.UpdatedBy,
        UpdatedAt = this.UpdatedAt,
        CreatedAt  = this.CreatedAt,
        IsAnalyzed  = this.IsAnalyzed,
        AnalyzedAt  = this.AnalyzedAt,
        RequiredPrivilege  = this.RequiredPrivilege,
        // Navigations
        UsedAsAvatar = this.UsedAsAvatar,
        UsedAsThumbnail = this.UsedAsThumbnail,
        FavoritedBy = this.FavoritedBy,
        Filepaths = this.Filepaths,
        PublicLinks = this.PublicLinks,
        Albums = this.Albums,
        Tags = this.Tags,
        UpdatedByNavigation = this.UpdatedByNavigation,
        UploadedByNavigation = this.UploadedByNavigation
    };

    /// <summary>
    /// Compare this <see cref="PhotoDTO"/> against its <see cref="Photo"/> equivalent.
    /// </summary>
    public bool Equals(Photo entity) {
        throw new NotImplementedException();
    }
}
