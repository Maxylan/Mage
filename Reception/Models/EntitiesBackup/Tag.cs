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
[Table("tags", Schema = "magedb")]
[Index("Name", Name = "idx_tags_name")]
[Index("Name", Name = "tags_name_key", IsUnique = true)]
*/
/*
public class Tag
{
    [Key]
    [JsonIgnore, SwaggerIgnore]
    public int Id { get; set; }

    [StringLength(127)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    // Methods

    [SwaggerIgnore]
    [JsonPropertyName("albums")]
    public int AlbumsCount => this.Albums?.Count() ?? 0;

    [SwaggerIgnore]
    [JsonPropertyName("photos")]
    public int PhotosCount => this.Photos?.Count() ?? 0;

    [SwaggerIgnore]
    public int Items => AlbumsCount + PhotosCount;

    // Navigation Properties

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("TagId")]
    [InverseProperty("AlbumTags")]
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("TagId")]
    [InverseProperty("Tags")]
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
*/
