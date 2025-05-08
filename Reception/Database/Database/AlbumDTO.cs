using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Album"/> data transfer object (DTO).
/// </summary>
public class AlbumDTO : Album, IDataTransferObject<Album>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("category_id")]
    public new int? CategoryId { get; set; }

    [JsonPropertyName("thumbnail_id")]
    public new int? ThumbnailId { get; set; }

    [JsonPropertyName("title")]
    public new string Title { get; set; } = null!;

    [JsonPropertyName("summary")]
    public new string? Summary { get; set; }

    [JsonPropertyName("description")]
    public new string? Description { get; set; }

    [JsonPropertyName("created_by")]
    public new int? CreatedBy { get; set; }

    [JsonPropertyName("created_at")]
    public new DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_by")]
    public new int? UpdatedBy { get; set; }

    [JsonPropertyName("updated_at")]
    public new DateTime UpdatedAt { get; set; }

    [JsonPropertyName("required_privilege")]
    public new byte RequiredPrivilege { get; set; }
    */

    /// <summary>
    /// Convert this <see cref="AlbumDTO"/> instance to its <see cref="Album"/> equivalent.
    /// </summary>
    public Album ToEntity() => new() {
        Id = this.Id ?? default,
        CategoryId = this.CategoryId,
        ThumbnailId = this.ThumbnailId,
        Title = this.Title,
        Summary = this.Summary,
        Description = this.Description,
        CreatedBy = this.CreatedBy,
        CreatedAt = this.CreatedAt,
        UpdatedBy = this.UpdatedBy,
        UpdatedAt = this.UpdatedAt,
        RequiredPrivilege = this.RequiredPrivilege
    };

    /// <summary>
    /// Compare this <see cref="AlbumDTO"/> against its <see cref="Album"/> equivalent.
    /// </summary>
    public bool Equals(Album entity) {
        throw new NotImplementedException();
    }
}
