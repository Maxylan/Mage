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
[Table("categories", Schema = "magedb")]
[Index("Title", Name = "categories_title_key", IsUnique = true)]
[Index("Title", Name = "idx_categories_title")]
[Index("UpdatedAt", Name = "idx_categories_updated_at")]
*/
/*
public partial class Category
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Description { get; set; }

    [SwaggerIgnore]
    public int? CreatedBy { get; set; }

    [SwaggerIgnore]
    [Column("created_at", TypeName = "TIMESTAMPZ")]
    public DateTime CreatedAt { get; set; }

    [SwaggerIgnore]
    [Column("updated_at", TypeName = "TIMESTAMPTZ")]
    public DateTime UpdatedAt { get; set; }

    // Methods

    [SwaggerIgnore]
    public int Items => Albums?.Count() ?? 0;

    // Navigation Properties

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("Category")]
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("CreatedBy")]
    [InverseProperty("Categories")]
    public virtual Account? CreatedByNavigation { get; set; }

    public static Action<EntityTypeBuilder<Category>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories", "magedb");

            entity.HasIndex(e => e.Title, "categories_title_key").IsUnique();

            entity.HasIndex(e => e.Title, "idx_categories_title");

            entity.HasIndex(e => e.UpdatedAt, "idx_categories_updated_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Summary)
                .HasMaxLength(255)
                .HasColumnName("summary");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Categories)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user");
        }
    );
}
*/
