using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Filepath"/> db-entity.
/// </summary>
[Table("filepaths", Schema = "magedb")]
[Index("Filename", Name = "idx_filepaths_filename")]
[Index("PhotoId", Name = "idx_filepaths_photo_id")]
[Index("Path", "Filename", Name = "idx_path_filename", IsUnique = true)]
public partial class Filepath : IDatabaseEntity<Filepath>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("photo_id")]
    public int PhotoId { get; set; }

    [Column("filename")]
    [StringLength(127)]
    public string Filename { get; set; } = null!;

    [Column("path")]
    [StringLength(255)]
    public string Path { get; set; } = null!;

    public Dimension? Dimension { get; set; }

    [Column("filesize")]
    public long? Filesize { get; set; }

    [Column("width")]
    public int? Width { get; set; }

    [Column("height")]
    public int? Height { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="Filepath"/> db-entity.
/// </summary>
public partial class Filepath
{
    [ForeignKey("PhotoId")]
    [InverseProperty("Filepaths")]
    public virtual Photo Photo { get; set; } = null!;

    public bool IsSource =>
        this.Dimension == Reception.Database.Dimension.SOURCE;

    public bool IsMedium =>
        this.Dimension == Reception.Database.Dimension.MEDIUM;

    public bool IsThumbnail =>
        this.Dimension == Reception.Database.Dimension.THUMBNAIL;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="Filepath"/>
    /// </summary>
    public static Action<EntityTypeBuilder<Filepath>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("filepaths_pkey");

            entity.HasOne(d => d.Photo).WithMany(p => p.Filepaths).HasConstraintName("fk_photo");

            entity.Property(e => e.Dimension)
                .HasColumnName("dimension")
                .HasDefaultValue(Reception.Database.Dimension.SOURCE)
                .HasSentinel(null)
                /* .HasConversion(
                    x => x.ToString() ?? Reception.Database.Models.Dimension.SOURCE.ToString(),
                    y => Enum.Parse<Dimension>(y, true)
                ) */;
        }
    );
}
