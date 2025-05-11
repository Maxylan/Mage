/*
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swashbuckle.AspNetCore.Annotations;
*/

// namespace Reception.Database.ModelsBackup;

/*
[Table("filepaths", Schema = "magedb")]
[Index("Filename", Name = "idx_filepaths_filename")]
[Index("PhotoId", Name = "idx_filepaths_photo_id")]
[Index("Path", "Filename", Name = "idx_path_filename", IsUnique = true)]
*/
/*
public class Filepath
{
    [Key]
    public int Id { get; set; }

    public int PhotoId { get; set; }

    [StringLength(127)]
    public string Filename { get; set; } = null!;

    [StringLength(255)]
    public string Path { get; set; } = null!;

    public Dimension? Dimension { get; set; }

    public long? Filesize { get; set; }

    public int? Height { get; set; }

    public int? Width { get; set; }

    // Method
    [SwaggerIgnore]
    public bool IsSource => this.Dimension == Reception.Database.Models.Dimension.SOURCE;

    // Navigation Properties

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("PhotoId")]
    [InverseProperty("Filepaths")]
    public virtual PhotoEntity Photo { get; set; } = null!;

    public static Action<EntityTypeBuilder<Filepath>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("filepaths_pkey");

            entity.ToTable("filepaths", "magedb");

            entity.HasIndex(e => e.Filename, "idx_filepaths_filename");

            entity.HasIndex(e => e.PhotoId, "idx_filepaths_photo_id");

            entity.HasIndex(e => new { e.Path, e.Filename }, "idx_path_filename").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Filename)
                .HasMaxLength(127)
                .HasColumnName("filename");
            entity.Property(e => e.Path)
                .HasMaxLength(255)
                .HasColumnName("path");
            entity.Property(e => e.Filesize).HasColumnName("filesize");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Dimension)
                .HasColumnName("dimension")
                .HasDefaultValue(Reception.Database.Models.Dimension.SOURCE)
                .HasSentinel(null)
                /* .HasConversion(
                    x => x.ToString() ?? Reception.Database.Models.Dimension.SOURCE.ToString(),
                    y => Enum.Parse<Dimension>(y, true)
                ) *//*;
            entity.Property(e => e.PhotoId).HasColumnName("photo_id");

            entity.HasOne(d => d.Photo).WithMany(p => p.Filepaths)
                .HasForeignKey(d => d.PhotoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_photo");
        }
    );
}
*/
