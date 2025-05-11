/*
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
*/

// namespace Reception.Database.ModelsBackup;

/*
[Table("photos", Schema = "magedb")]
[Index("Slug", Name = "idx_photos_slug")]
[Index("UpdatedAt", Name = "idx_photos_updated_at")]
[Index("Slug", Name = "photos_slug_key", IsUnique = true)]
*/
/*
public class PhotoEntity
{
    [Key]
    public int Id { get; set; }
    public string Slug { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string? Description { get; set; }

    [SwaggerIgnore]
    public int? UploadedBy { get; set; }

    [SwaggerIgnore]
    [Column("uploaded_at", TypeName = "TIMESTAMPTZ")]
    public DateTime UploadedAt { get; set; }

    [SwaggerIgnore]
    [Column("updated_at", TypeName = "TIMESTAMPTZ")]
    public DateTime UpdatedAt { get; set; }

    [SwaggerIgnore]
    [Column("created_at", TypeName = "TIMESTAMP")]
    public DateTime CreatedAt { get; set; }

    // Navigation Properties

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("Avatar")]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("Thumbnail")]
    public virtual ICollection<Album> ThumbnailForAlbums { get; set; } = new List<Album>();

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("Photo")]
    public virtual ICollection<Filepath> Filepaths { get; set; } = new List<Filepath>();

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("Photo")]
    public virtual ICollection<Link> Links { get; set; } = new List<Link>();

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("UploadedBy")]
    [InverseProperty("UploadedPhotos")]
    public virtual Account? UploadedByNavigation { get; set; }

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("PhotoId")]
    [InverseProperty("Photos")]
    public virtual ICollection<Album> AlbumsNavigation { get; set; } = new List<Album>();

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("PhotoId")]
    [InverseProperty("Photos")]
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
                .HasColumnName("created_at")
                .HasColumnType("TIMESTAMP");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Slug)
                .HasMaxLength(127)
                .HasColumnName("slug");
            entity.Property(e => e.Summary)
                .HasMaxLength(255)
                .HasColumnName("summary");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("uploaded_at")
                .HasColumnType("TIMESTAMPTZ");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at")
                .HasColumnType("TIMESTAMPTZ");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.UploadedPhotos)
                .HasForeignKey(d => d.UploadedBy)
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
*/
