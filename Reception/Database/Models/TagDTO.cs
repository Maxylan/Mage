using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// Convert this <see cref="TagDTO"/> instance to its <see cref="Tag"/> equivalent.
    /// </summary>
    public Tag ToEntity() => new() {
        Id = this.Id ?? default,
        Name = this.Name,
        Description  = this.Description,
        RequiredPrivilege  = this.RequiredPrivilege
    };

    /// <summary>
    /// Compare this <see cref="TagDTO"/> against its <see cref="Tag"/> equivalent.
    /// </summary>
    public bool Equals(Tag entity) {
        throw new NotImplementedException();
    }
}
