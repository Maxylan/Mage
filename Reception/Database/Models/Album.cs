using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Album"/> db-entity.
/// </summary>
[Table("albums", Schema = "magedb")]
[Index("Title", Name = "albums_title_key", IsUnique = true)]
[Index("Title", Name = "idx_albums_title")]
[Index("UpdatedAt", Name = "idx_albums_updated_at")]
public partial class Album : IDatabaseEntity<Album>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("thumbnail_id")]
    public int? ThumbnailId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("summary")]
    [StringLength(255)]
    public string? Summary { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("required_privilege")]
    public byte RequiredPrivilege { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="Album"/> db-entity.
/// </summary>
public partial class Album
{
    [InverseProperty("Album")]
    public virtual ICollection<AlbumTagRelation> Tags { get; set; } = new List<AlbumTagRelation>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Albums")]
    public virtual Category? Category { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("AlbumsCreated")]
    public virtual Account? CreatedByNavigation { get; set; }

    [InverseProperty("Album")]
    public virtual ICollection<FavoriteAlbumRelation> FavoritedBy { get; set; } = new List<FavoriteAlbumRelation>();

    [InverseProperty("Album")]
    public virtual ICollection<PhotoAlbumRelation> Photos { get; set; } = new List<PhotoAlbumRelation>();

    [ForeignKey("ThumbnailId")]
    [InverseProperty("UsedAsThumbnail")]
    public virtual Photo? Thumbnail { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("AlbumsUpdated")]
    public virtual Account? UpdatedByNavigation { get; set; }    /// <summary>

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="Album"/>
    /// </summary>
    public static Action<EntityTypeBuilder<Album>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("albums_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.RequiredPrivilege).HasDefaultValue((short)0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Category).WithMany(p => p.Albums)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_category");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AlbumsCreated)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_created_by_user");

            entity.HasOne(d => d.Thumbnail).WithMany(p => p.UsedAsThumbnail)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_thumbnail");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AlbumsUpdated)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_updated_by_user");
        }
    );
}
