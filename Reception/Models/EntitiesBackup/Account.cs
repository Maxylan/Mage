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
[Table("accounts", Schema = "magedb")]
[Index("Email", Name = "accounts_email_key", IsUnique = true)]
[Index("Username", Name = "accounts_username_key", IsUnique = true)]
[Index("Email", Name = "idx_accounts_email", IsUnique = true)]
[Index("LastVisit", Name = "idx_accounts_last_visit")]
[Index("Username", Name = "idx_accounts_username", IsUnique = true)]
*/
/*
public class Account
{
    [Key]
    public int Id { get; set; }

    public string? Email { get; set; }

    public string Username { get; set; } = null!;

    [JsonIgnore, SwaggerIgnore]
    public string Password { get; set; } = null!;

    public int? AvatarId { get; set; }

    public string? FullName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastVisit { get; set; }

    public byte Permissions { get; set; }

    // Navigation Properties

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("AvatarId")]
    [InverseProperty("Accounts")]
    public virtual PhotoEntity? Avatar { get; set; }

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Link> Links { get; set; } = new List<Link>();

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("UploadedByNavigation")]
    public virtual ICollection<PhotoEntity> UploadedPhotos { get; set; } = new List<PhotoEntity>();

    [JsonIgnore, SwaggerIgnore]
    [InverseProperty("User")]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public static Action<EntityTypeBuilder<Account>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("accounts_pkey");

            entity.ToTable("accounts", "magedb");

            entity.HasIndex(e => e.Email, "accounts_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "accounts_username_key").IsUnique();

            entity.HasIndex(e => e.Email, "idx_accounts_email").IsUnique();

            entity.HasIndex(e => e.LastVisit, "idx_accounts_last_visit");

            entity.HasIndex(e => e.Username, "idx_accounts_username").IsUnique();

            entity.Property(e => e.AvatarId).HasColumnName("avatar_id");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(127)
                .HasColumnName("full_name");
            entity.Property(e => e.LastVisit)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_visit");
            entity.Property(e => e.Password)
                .HasMaxLength(127)
                .HasColumnName("password");
            entity.Property(e => e.Permissions)
                .HasDefaultValue((short)0)
                .HasColumnName("permissions");
            entity.Property(e => e.Username)
                .HasMaxLength(63)
                .HasColumnName("username");

            entity.HasOne(d => d.Avatar).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.AvatarId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_avatar");
        }
    );
}
*/
