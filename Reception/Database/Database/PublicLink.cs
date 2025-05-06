using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="PublicLink"/> db-entity.
/// </summary>
[Table("links", Schema = "magedb")]
[Index("Code", Name = "idx_links_code")]
[Index("PhotoId", Name = "idx_links_photo_id")]
[Index("Code", Name = "links_code_key", IsUnique = true)]
public partial class PublicLink : IDatabaseEntity<PublicLink>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("photo_id")]
    public int PhotoId { get; set; }

    [Column("code")]
    [StringLength(32)]
    public string Code { get; set; } = null!;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("access_limit")]
    public int? AccessLimit { get; set; }

    [Column("accessed")]
    public int Accessed { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="PublicLink"/> db-entity.
/// </summary>
public partial class PublicLink
{
    [ForeignKey("CreatedBy")]
    [InverseProperty("LinksCreated")]
    public virtual Account? CreatedByNavigation { get; set; }

    [ForeignKey("PhotoId")]
    [InverseProperty("PublicLinks")]
    public virtual Photo Photo { get; set; } = null!;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="PublicLink"/>
    /// </summary>
    public static Action<EntityTypeBuilder<PublicLink>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("links_pkey");

            entity.Property(e => e.Accessed).HasDefaultValue(0);
            entity.Property(e => e.Code).IsFixedLength();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.LinksCreated)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_user");

            entity.HasOne(d => d.Photo).WithMany(p => p.PublicLinks).HasConstraintName("fk_photo");
        }
    );
}
