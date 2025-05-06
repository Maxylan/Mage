using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Filepath"/> data transfer object (DTO).
/// </summary>
public class FilepathDTO : Filepath, IDataTransferObject<Filepath>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

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

    /// <summary>
    /// Convert this <see cref="FilepathDTO"/> instance to its <see cref="Filepath"/> equivalent.
    /// </summary>
    public Filepath ToEntity() => new() {
        Id = this.Id ?? default,
        PhotoId = this.PhotoId,
        Filename = this.Filename,
        Path = this.Path,
        Filesize = this.Filesize,
        Width = this.Width,
        Height = this.Height
    };

    /// <summary>
    /// Compare this <see cref="FilepathDTO"/> against its <see cref="Filepath"/> equivalent.
    /// </summary>
    public bool Equals(Filepath entity) {
        throw new NotImplementedException();
    }
}
