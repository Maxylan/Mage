using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="AlbumTagRelation"/> db-entity.
/// </summary>
[PrimaryKey("PhotoId", "AlbumId")]
[Table("photo_albums", Schema = "magedb")]
[Index("AlbumId", Name = "idx_photo_albums_album_id")]
[Index("PhotoId", Name = "idx_photo_albums_photo_id")]
public partial class PhotoAlbumRelation : IDatabaseEntity<PhotoAlbumRelation>
{
    [Key]
    [Column("photo_id")]
    public int PhotoId { get; set; }

    [Key]
    [Column("album_id")]
    public int AlbumId { get; set; }

    [Column("added")]
    public DateTime Added { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="PhotoAlbumRelation"/> db-entity.
/// </summary>
public partial class PhotoAlbumRelation
{
    [ForeignKey("AlbumId")]
    [InverseProperty("Photos")]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey("PhotoId")]
    [InverseProperty("Albums")]
    public virtual Photo Photo { get; set; } = null!;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="PhotoAlbumRelation"/>
    /// </summary>
    public static Action<EntityTypeBuilder<PhotoAlbumRelation>> Build => (
        entity =>
        {
            entity.HasKey(e => new { e.PhotoId, e.AlbumId }).HasName("photo_albums_pkey");

            entity.Property(e => e.Added).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Album).WithMany(p => p.Photos).HasConstraintName("fk_album");

            entity.HasOne(d => d.Photo).WithMany(p => p.Albums).HasConstraintName("fk_photo");
        }
    );
}
