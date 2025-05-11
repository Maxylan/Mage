using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="FavoriteAlbumRelation"/> db-entity.
/// </summary>
[PrimaryKey("AccountId", "AlbumId")]
[Table("favorite_albums", Schema = "magedb")]
[Index("AccountId", Name = "idx_favorite_albums_account_id")]
[Index("AlbumId", Name = "idx_favorite_albums_album_id")]
public partial class FavoriteAlbumRelation : IDatabaseEntity<FavoriteAlbumRelation>
{
    [Key]
    [Column("account_id")]
    public int AccountId { get; set; }

    [Key]
    [Column("album_id")]
    public int AlbumId { get; set; }

    [Column("added")]
    public DateTime Added { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="FavoriteAlbumRelation"/> db-entity.
/// </summary>
public partial class FavoriteAlbumRelation
{
    [ForeignKey("AccountId")]
    [InverseProperty("FavoriteAlbums")]
    public virtual Account Account { get; set; } = null!;

    [ForeignKey("AlbumId")]
    [InverseProperty("FavoritedBy")]
    public virtual Album Album { get; set; } = null!;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="FavoriteAlbumRelation"/>
    /// </summary>
    public static Action<EntityTypeBuilder<FavoriteAlbumRelation>> Build => (
        entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.AlbumId }).HasName("favorite_albums_pkey");

            entity.Property(e => e.Added).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Account).WithMany(p => p.FavoriteAlbums).HasConstraintName("fk_account");

            entity.HasOne(d => d.Album).WithMany(p => p.FavoritedBy).HasConstraintName("fk_album");
        }
    );
}
