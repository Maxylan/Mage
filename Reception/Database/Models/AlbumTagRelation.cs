using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="AlbumTagRelation"/> db-entity.
/// </summary>
[PrimaryKey("AlbumId", "TagId")]
[Table("album_tags", Schema = "magedb")]
[Index("AlbumId", Name = "idx_album_tags_album_id")]
[Index("TagId", Name = "idx_album_tags_tag_id")]
public partial class AlbumTagRelation : IDatabaseEntity<AlbumTagRelation>
{
    [Key]
    [Column("album_id")]
    public int AlbumId { get; set; }

    [Key]
    [Column("tag_id")]
    public int TagId { get; set; }

    [Column("added")]
    public DateTime Added { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="AlbumTagRelation"/> db-entity.
/// </summary>
public partial class AlbumTagRelation
{
    [ForeignKey("AlbumId")]
    [InverseProperty("Tags")]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey("TagId")]
    [InverseProperty("UsedByAlbums")]
    public virtual Tag Tag { get; set; } = null!;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="AlbumTagRelation"/>
    /// </summary>
    public static Action<EntityTypeBuilder<AlbumTagRelation>> Build => (
        entity =>
        {
            entity.HasKey(e => new { e.AlbumId, e.TagId }).HasName("album_tags_pkey");

            entity.Property(e => e.Added).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Album).WithMany(p => p.Tags).HasConstraintName("fk_album");

            entity.HasOne(d => d.Tag).WithMany(p => p.UsedByAlbums).HasConstraintName("fk_tag");
        }
    );
}
