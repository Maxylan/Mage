using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="FavoritePhotoRelation"/> db-entity.
/// </summary>
[PrimaryKey("AccountId", "PhotoId")]
[Table("favorite_photos", Schema = "magedb")]
[Index("AccountId", Name = "idx_favorite_photos_account_id")]
[Index("PhotoId", Name = "idx_favorite_photos_photo_id")]
public partial class FavoritePhotoRelation : IDatabaseEntity<FavoritePhotoRelation>
{
    [Key]
    [Column("account_id")]
    public int AccountId { get; set; }

    [Key]
    [Column("photo_id")]
    public int PhotoId { get; set; }

    [Column("added")]
    public DateTime Added { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="FavoritePhotoRelation"/> db-entity.
/// </summary>
public partial class FavoritePhotoRelation
{
    [ForeignKey("AccountId")]
    [InverseProperty("FavoritePhotos")]
    public virtual Account Account { get; set; } = null!;

    [ForeignKey("PhotoId")]
    [InverseProperty("FavoritedBy")]
    public virtual Photo Photo { get; set; } = null!;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="FavoritePhotoRelation"/>
    /// </summary>
    public static Action<EntityTypeBuilder<FavoritePhotoRelation>> Build => (
        entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.PhotoId }).HasName("favorite_photos_pkey");

            entity.Property(e => e.Added).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Account).WithMany(p => p.FavoritePhotos).HasConstraintName("fk_account");

            entity.HasOne(d => d.Photo).WithMany(p => p.FavoritedBy).HasConstraintName("fk_photo");
        }
    );
}
