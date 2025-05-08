using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        RequiredPrivilege  = this.RequiredPrivilege
    };

    /// <summary>
    /// Compare this <see cref="PhotoDTO"/> against its <see cref="Photo"/> equivalent.
    /// </summary>
    public bool Equals(Photo entity) {
        throw new NotImplementedException();
    }
}
