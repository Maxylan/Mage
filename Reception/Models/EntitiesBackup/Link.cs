/*
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reception.Services.DataAccess;
using Swashbuckle.AspNetCore.Annotations;
*/

// namespace Reception.Database.ModelsBackup;

/*
[Table("links", Schema = "magedb")]
[Index("Code", Name = "idx_links_code")]
[Index("Code", Name = "links_code_key", IsUnique = true)]
[Index("PhotoId", Name = "idx_links_photo_id")]
*/
/*
public class Link
{
    [Key]
    public int Id { get; set; }

    public int PhotoId { get; set; }

    [StringLength(32)]
    public string Code { get; set; } = null!;

    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("access_limit")]
    public int? AccessLimit { get; set; }

    [Column("accessed")]
    public int Accessed { get; set; }

    // Method
    [SwaggerIgnore]
    public string Uri => LinkService.GenerateLinkUri(this.Code).ToString();

    [SwaggerIgnore]
    public bool Active => (
        this.ExpiresAt > DateTime.UtcNow && (
            this.AccessLimit is null || this.AccessLimit <= 0 || this.Accessed < this.AccessLimit
        )
    );

    // Navigation Properties

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("CreatedBy")]
    [InverseProperty("Links")]
    public virtual Account? CreatedByNavigation { get; set; }

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("PhotoId")]
    [InverseProperty("Links")]
    public virtual PhotoEntity Photo { get; set; } = null!;

    public static Action<EntityTypeBuilder<Link>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("links_pkey");

            entity.HasIndex(e => e.Code, "idx_links_code").IsUnique();

            entity.HasIndex(e => e.PhotoId, "idx_links_photo_id");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Code)
                .HasColumnName("code")
                .HasMaxLength(32)
                .IsFixedLength();

            entity.Property(e => e.PhotoId).HasColumnName("photo_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.AccessLimit).HasColumnName("access_limit");
            entity.Property(e => e.Accessed)
                .HasColumnName("accessed")
                .HasDefaultValue(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Links)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_user");

            entity.HasOne(d => d.Photo).WithMany(p => p.Links)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_photo");
        }
    );
}
*/
