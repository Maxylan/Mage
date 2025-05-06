using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="FavoriteAlbumRelation"/> data transfer object (DTO).
/// </summary>
public class FavoriteAlbumRelationDTO : FavoriteAlbumRelation, IDataTransferObject<FavoriteAlbumRelation>
{
    [JsonPropertyName("account_id")]
    public new int AccountId { get; set; }

    [JsonPropertyName("album_id")]
    public new int AlbumId { get; set; }

    [JsonPropertyName("added")]
    public new DateTime Added { get; set; }

    /// <summary>
    /// Convert this <see cref="FavoriteAlbumRelationDTO"/> instance to its <see cref="FavoriteAlbumRelation"/> equivalent.
    /// </summary>
    public FavoriteAlbumRelation ToEntity() => new() {
        AccountId = this.AccountId,
        AlbumId = this.AlbumId,
        Added = this.Added
    };

    /// <summary>
    /// Compare this <see cref="FavoriteAlbumRelationDTO"/> against its <see cref="FavoriteAlbumRelation"/> equivalent.
    /// </summary>
    public bool Equals(FavoriteAlbumRelation entity) {
        throw new NotImplementedException();
    }
}
