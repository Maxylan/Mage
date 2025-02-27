using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Models;

public class Album
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public int? ThumbnailId { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Photo? Thumbnail { get; set; }

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public static Action<EntityTypeBuilder<Album>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("albums_pkey");

            entity.ToTable("albums", "magedb");

            entity.HasIndex(e => e.Title, "albums_title_key").IsUnique();

            entity.HasIndex(e => e.Title, "idx_albums_title");

            entity.HasIndex(e => e.UpdatedAt, "idx_albums_updated_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Summary)
                .HasMaxLength(255)
                .HasColumnName("summary");
            entity.Property(e => e.ThumbnailId).HasColumnName("thumbnail_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Albums)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_category");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Albums)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user");

            entity.HasOne(d => d.Thumbnail).WithMany(p => p.Albums)
                .HasForeignKey(d => d.ThumbnailId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_thumbnail");

            entity.HasMany(d => d.Tags).WithMany(p => p.Albums)
                .UsingEntity<Dictionary<string, object>>(
                    "AlbumTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("fk_tag"),
                    l => l.HasOne<Album>().WithMany()
                        .HasForeignKey("AlbumId")
                        .HasConstraintName("fk_album"),
                    j =>
                    {
                        j.HasKey("AlbumId", "TagId").HasName("album_tags_pkey");
                        j.ToTable("album_tags", "magedb");
                        j.HasIndex(new[] { "AlbumId" }, "idx_album_tags_album_id");
                        j.HasIndex(new[] { "TagId" }, "idx_album_tags_tag_id");
                        j.IndexerProperty<int>("AlbumId").HasColumnName("album_id");
                        j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                    });
        }
    );
}
