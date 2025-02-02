using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Models.Entities;

public class Filepath
{
    public int Id { get; set; }
    public int PhotoId { get; set; }
    public string Filename { get; set; } = null!;
    public string Path { get; set; } = null!;
    public Dimension? Dimension { get; set; }
    public long? Filesize { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }

    // Method
    public bool IsSource => this.Dimension == Reception.Models.Entities.Dimension.SOURCE;

    // Navigation Properties

    [JsonIgnore]
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
                .HasDefaultValue(Reception.Models.Entities.Dimension.SOURCE)
                .HasSentinel(null)
                /* .HasConversion(
                    x => x.ToString() ?? Reception.Models.Entities.Dimension.SOURCE.ToString(),
                    y => Enum.Parse<Dimension>(y, true)
                ) */;
            entity.Property(e => e.PhotoId).HasColumnName("photo_id");

            entity.HasOne(d => d.Photo).WithMany(p => p.Filepaths)
                .HasForeignKey(d => d.PhotoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_photo");
        }
    );
}
