using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="PhotoTagRelation"/> db-entity.
/// </summary>
[PrimaryKey("PhotoId", "TagId")]
[Table("photo_tags", Schema = "magedb")]
[Index("PhotoId", Name = "idx_photo_tags_photo_id")]
[Index("TagId", Name = "idx_photo_tags_tag_id")]
public partial class PhotoTagRelation : IDatabaseEntity<PhotoTagRelation>
{
    [Key]
    [Column("photo_id")]
    public int PhotoId { get; set; }

    [Key]
    [Column("tag_id")]
    public int TagId { get; set; }

    [Column("added")]
    public DateTime Added { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="PhotoTagRelation"/> db-entity.
/// </summary>
public partial class PhotoTagRelation
{
    [ForeignKey("PhotoId")]
    [InverseProperty("Tags")]
    public virtual Photo Photo { get; set; } = null!;

    [ForeignKey("TagId")]
    [InverseProperty("UsedByPhotos")]
    public virtual Tag Tag { get; set; } = null!;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="PhotoTagRelation"/>
    /// </summary>
    public static Action<EntityTypeBuilder<PhotoTagRelation>> Build => (
        entity =>
        {
            entity.HasKey(e => new { e.PhotoId, e.TagId }).HasName("photo_tags_pkey");

            entity.Property(e => e.Added).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Photo).WithMany(p => p.Tags).HasConstraintName("fk_photo");

            entity.HasOne(d => d.Tag).WithMany(p => p.UsedByPhotos).HasConstraintName("fk_tag");
        }
    );
}
