using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Category"/> data transfer object (DTO).
/// </summary>
public class CategoryDTO : Category, IDataTransferObject<Category>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

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

    [JsonPropertyName("updated_at")]
    public new DateTime UpdatedAt { get; set; }

    [JsonPropertyName("required_privilege")]
    public new byte RequiredPrivilege { get; set; }

    /// <summary>
    /// Convert this <see cref="CategoryDTO"/> instance to its <see cref="Category"/> equivalent.
    /// </summary>
    public Category ToEntity() => new() {
        Id = this.Id ?? default,
        Title = this.Title,
        Summary = this.Summary,
        Description = this.Description,
        CreatedBy = this.CreatedBy,
        CreatedAt = this.CreatedAt,
        UpdatedAt = this.UpdatedAt,
        RequiredPrivilege = this.RequiredPrivilege
    };

    /// <summary>
    /// Compare this <see cref="CategoryDTO"/> against its <see cref="Category"/> equivalent.
    /// </summary>
    public bool Equals(Category entity) {
        throw new NotImplementedException();
    }
}
