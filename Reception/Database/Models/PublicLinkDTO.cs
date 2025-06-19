using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="PublicLink"/> data transfer object (DTO).
/// </summary>
public class PublicLinkDTO : PublicLink, IDataTransferObject<PublicLink>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("photo_id")]
    public new int PhotoId { get; set; }

    [JsonPropertyName("code")]
    public new string Code { get; set; } = null!;

    [JsonPropertyName("created_by")]
    public new int? CreatedBy { get; set; }

    [JsonPropertyName("created_at")]
    public new DateTime CreatedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public new DateTime ExpiresAt { get; set; }

    [JsonPropertyName("access_limit")]
    public new int? AccessLimit { get; set; }

    [JsonPropertyName("accessed")]
    public new int Accessed { get; set; }
    */

    [JsonIgnore, SwaggerIgnore]
    public new Account? CreatedByNavigation { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new Photo Photo { get; set; } = null!;

    /// <summary>
    /// Convert this <see cref="PublicLinkDTO"/> instance to its <see cref="PublicLink"/> equivalent.
    /// </summary>
    public PublicLink ToEntity() => new() {
        Id = this.Id ?? default,
        PhotoId = this.PhotoId,
        Code = this.Code,
        CreatedBy = this.CreatedBy,
        CreatedAt = this.CreatedAt,
        ExpiresAt = this.ExpiresAt,
        AccessLimit = this.AccessLimit,
        Accessed = this.Accessed,
        // Navigations
        CreatedByNavigation = this.CreatedByNavigation,
        Photo = this.Photo
    };

    /// <summary>
    /// Compare this <see cref="PublicLinkDTO"/> against its <see cref="PublicLink"/> equivalent.
    /// </summary>
    public bool Equals(PublicLink entity) {
        throw new NotImplementedException();
    }
}
