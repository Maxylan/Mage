using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Reception.Models.Entities;

public class PhotoEntity
{
    public int Id { get; set; }
    public string Slug { get; set; } = null!;
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties

    [JsonIgnore]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [JsonIgnore]
    public virtual ICollection<Album> ThumbnailForAlbums { get; set; } = new List<Album>();

    [JsonIgnore]
    public virtual Account? CreatedByNavigation { get; set; }

    [JsonIgnore]
    public virtual ICollection<Filepath> Filepaths { get; set; } = new List<Filepath>();

    [JsonIgnore]
    public virtual ICollection<Album> AlbumsNavigation { get; set; } = new List<Album>();

    [JsonIgnore]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public static Action<EntityTypeBuilder<PhotoEntity>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("photos_pkey");

            entity.ToTable("photos", "magedb");

            entity.HasIndex(e => e.Slug, "idx_photos_slug");

            entity.HasIndex(e => e.UpdatedAt, "idx_photos_updated_at");

            entity.HasIndex(e => e.Slug, "photos_slug_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Slug)
                .HasMaxLength(127)
                .HasColumnName("name");
            entity.Property(e => e.Summary)
                .HasMaxLength(255)
                .HasColumnName("summary");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Photos)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user");

            entity.HasMany(d => d.AlbumsNavigation).WithMany(p => p.Photos)
                .UsingEntity<Dictionary<string, object>>(
                    "PhotoAlbum",
                    r => r.HasOne<Album>().WithMany()
                        .HasForeignKey("AlbumId")
                        .HasConstraintName("fk_album"),
                    l => l.HasOne<PhotoEntity>().WithMany()
                        .HasForeignKey("PhotoId")
                        .HasConstraintName("fk_photo"),
                    j =>
                    {
                        j.HasKey("PhotoId", "AlbumId").HasName("photo_albums_pkey");
                        j.ToTable("photo_albums", "magedb");
                        j.HasIndex(new[] { "AlbumId" }, "idx_photo_albums_album_id");
                        j.HasIndex(new[] { "PhotoId" }, "idx_photo_albums_photo_id");
                        j.IndexerProperty<int>("PhotoId").HasColumnName("photo_id");
                        j.IndexerProperty<int>("AlbumId").HasColumnName("album_id");
                    });

            entity.HasMany(d => d.Tags).WithMany(p => p.Photos)
                .UsingEntity<Dictionary<string, object>>(
                    "PhotoTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("fk_tag"),
                    l => l.HasOne<PhotoEntity>().WithMany()
                        .HasForeignKey("PhotoId")
                        .HasConstraintName("fk_photo"),
                    j =>
                    {
                        j.HasKey("PhotoId", "TagId").HasName("photo_tags_pkey");
                        j.ToTable("photo_tags", "magedb");
                        j.HasIndex(new[] { "PhotoId" }, "idx_photo_tags_photo_id");
                        j.HasIndex(new[] { "TagId" }, "idx_photo_tags_tag_id");
                        j.IndexerProperty<int>("PhotoId").HasColumnName("photo_id");
                        j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                    });
        }
    );
}
