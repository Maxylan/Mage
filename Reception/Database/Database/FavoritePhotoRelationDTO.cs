using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="FavoritePhotoRelation"/> data transfer object (DTO).
/// </summary>
public class FavoritePhotoRelationDTO : FavoritePhotoRelation, IDataTransferObject<FavoritePhotoRelation>
{
    [JsonPropertyName("account_id")]
    public new int AccountId { get; set; }

    [JsonPropertyName("photo_id")]
    public new int PhotoId { get; set; }

    [JsonPropertyName("added")]
    public new DateTime Added { get; set; }

    /// <summary>
    /// Convert this <see cref="FavoritePhotoRelationDTO"/> instance to its <see cref="FavoritePhotoRelation"/> equivalent.
    /// </summary>
    public FavoritePhotoRelation ToEntity() => new() {
        AccountId = this.AccountId,
        PhotoId = this.PhotoId,
        Added = this.Added
    };

    /// <summary>
    /// Compare this <see cref="FavoritePhotoRelationDTO"/> against its <see cref="FavoritePhotoRelation"/> equivalent.
    /// </summary>
    public bool Equals(FavoritePhotoRelation entity) {
        throw new NotImplementedException();
    }
}
