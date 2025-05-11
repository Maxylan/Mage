using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Photo"/> db-entity.
/// </summary>
[Table("photos", Schema = "magedb")]
[Index("Slug", Name = "idx_photos_slug")]
[Index("UpdatedAt", Name = "idx_photos_updated_at")]
[Index("Slug", Name = "photos_slug_key", IsUnique = true)]
public partial class Photo : IDatabaseEntity<Photo>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("slug")]
    [StringLength(127)]
    public string Slug { get; set; } = null!;

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("summary")]
    [StringLength(255)]
    public string? Summary { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("uploaded_by")]
    public int? UploadedBy { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("is_analyzed")]
    public bool IsAnalyzed { get; set; }

    [Column("analyzed_at", TypeName = "timestamp without time zone")]
    public DateTime? AnalyzedAt { get; set; }

    [Column("required_privilege")]
    public byte RequiredPrivilege { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="Photo"/> db-entity.
/// </summary>
public partial class Photo
{
    [InverseProperty("Avatar")]
    public virtual ICollection<Account> UsedAsAvatar { get; set; } = new List<Account>();

    [InverseProperty("Thumbnail")]
    public virtual ICollection<Album> UsedAsThumbnail { get; set; } = new List<Album>();

    [InverseProperty("Photo")]
    public virtual ICollection<FavoritePhotoRelation> FavoritedBy { get; set; } = new List<FavoritePhotoRelation>();

    [InverseProperty("Photo")]
    public virtual ICollection<Filepath> Filepaths { get; set; } = new List<Filepath>();

    [InverseProperty("Photo")]
    public virtual ICollection<PublicLink> PublicLinks { get; set; } = new List<PublicLink>();

    [InverseProperty("Photo")]
    public virtual ICollection<PhotoAlbumRelation> Albums { get; set; } = new List<PhotoAlbumRelation>();

    [InverseProperty("Photo")]
    public virtual ICollection<PhotoTagRelation> Tags { get; set; } = new List<PhotoTagRelation>();

    [SwaggerIgnore]
    [ForeignKey("UpdatedBy")]
    [InverseProperty("PhotosUpdated")]
    public virtual Account? UpdatedByNavigation { get; set; }

    [SwaggerIgnore]
    [ForeignKey("UploadedBy")]
    [InverseProperty("PhotosUploaded")]
    public virtual Account? UploadedByNavigation { get; set; }

    // Lil' helpers
    [SwaggerIgnore]
    public bool SourceExists =>
        this.Filepaths?.Any(path => path.Dimension == Dimension.SOURCE) == true;
    [SwaggerIgnore]
    public bool MediumExists =>
        this.Filepaths?.Any(path => path.Dimension == Dimension.MEDIUM) == true;
    [SwaggerIgnore]
    public bool ThumbnailExists =>
        this.Filepaths?.Any(path => path.Dimension == Dimension.THUMBNAIL) == true;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="Photo"/>
    /// </summary>
    public static Action<EntityTypeBuilder<Photo>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("photos_pkey");

            entity.Property(e => e.IsAnalyzed).HasDefaultValue(false);
            entity.Property(e => e.RequiredPrivilege).HasDefaultValue((short)0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.PhotosUpdated)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_updated_by_user");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.PhotosUploaded)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_uploaded_by_user");
        }
    );
}
