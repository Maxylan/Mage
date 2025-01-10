using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Models.Entities;

public partial class Category
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

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
