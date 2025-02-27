using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Models.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation Properties

    [JsonIgnore]
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    [JsonIgnore]
    public virtual ICollection<PhotoEntity> Photos { get; set; } = new List<PhotoEntity>();

    public static Action<EntityTypeBuilder<Tag>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.ToTable("tags", "magedb");

            entity.HasIndex(e => e.Name, "idx_tags_name");

            entity.HasIndex(e => e.Name, "tags_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(127)
                .HasColumnName("name");
        }
    );
}
