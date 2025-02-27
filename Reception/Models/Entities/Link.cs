using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Models.Entities;

[Table("links", Schema = "magedb")]
[Index("Code", Name = "idx_links_code")]
[Index("Code", Name = "links_code_key", IsUnique = true)]
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

            entity.Property(e => e.Accessed).HasDefaultValue(0);
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Links)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_user");

            entity.HasOne(d => d.Photo).WithMany(p => p.Links).HasConstraintName("fk_photo");
        }
    );
}
