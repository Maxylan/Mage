using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Account"/> db-entity.
/// </summary>
[Table("accounts", Schema = "magedb")]
[Index("Email", Name = "accounts_email_key", IsUnique = true)]
[Index("Username", Name = "accounts_username_key", IsUnique = true)]
[Index("Email", Name = "idx_accounts_email", IsUnique = true)]
[Index("LastLogin", Name = "idx_accounts_last_login")]
[Index("Username", Name = "idx_accounts_username", IsUnique = true)]
public partial class Account : IDatabaseEntity<Account>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Column("username")]
    [StringLength(127)]
    public string Username { get; set; } = null!;

    [Column("password")]
    [StringLength(127)]
    public string Password { get; set; } = null!;

    [Column("full_name")]
    [StringLength(255)]
    public string? FullName { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_login")]
    public DateTime LastLogin { get; set; }

    [Column("privilege")]
    public short Privilege { get; set; }

    [Column("avatar_id")]
    public int? AvatarId { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="Account"/> db-entity.
/// </summary>
public partial class Account
{
    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Album> AlbumsCreated { get; set; } = new List<Album>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Album> AlbumsUpdated { get; set; } = new List<Album>();

    [ForeignKey("AvatarId")]
    [InverseProperty("UsedAsAvatar")]
    public virtual Photo? Avatar { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Category> CreatedCategories { get; set; } = new List<Category>();

    [InverseProperty("Account")]
    public virtual ICollection<FavoriteAlbumRelation> FavoriteAlbums { get; set; } = new List<FavoriteAlbumRelation>();

    [InverseProperty("Account")]
    public virtual ICollection<FavoritePhotoRelation> FavoritePhotos { get; set; } = new List<FavoritePhotoRelation>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<PublicLink> LinksCreated { get; set; } = new List<PublicLink>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Photo> PhotosUpdated { get; set; } = new List<Photo>();

    [InverseProperty("UploadedByNavigation")]
    public virtual ICollection<Photo> PhotosUploaded { get; set; } = new List<Photo>();

    [InverseProperty("Account")]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    [ForeignKey("AccountId")]
    [InverseProperty("Accounts")]
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="Account"/>
    /// </summary>
    public static Action<EntityTypeBuilder<Account>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("accounts_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.LastLogin).HasDefaultValueSql("now()");
            entity.Property(e => e.Privilege).HasDefaultValue((short)0);

            entity.HasOne(d => d.Avatar).WithMany(p => p.UsedAsAvatar)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_avatar");

            entity.HasMany(d => d.Clients).WithMany(p => p.Accounts)
                .UsingEntity<Dictionary<string, object>>(
                    "AccountClient",
                    r => r.HasOne<Client>().WithMany()
                        .HasForeignKey("ClientId")
                        .HasConstraintName("fk_client"),
                    l => l.HasOne<Account>().WithMany()
                        .HasForeignKey("AccountId")
                        .HasConstraintName("fk_account"),
                    j =>
                    {
                        j.HasKey("AccountId", "ClientId").HasName("account_clients_pkey");
                        j.ToTable("account_clients", "magedb");
                        j.HasIndex(new[] { "AccountId" }, "idx_account_clients_account_id");
                        j.HasIndex(new[] { "ClientId" }, "idx_account_clients_photo_id");
                        j.IndexerProperty<int>("AccountId").HasColumnName("account_id");
                        j.IndexerProperty<int>("ClientId").HasColumnName("client_id");
                    });
        }
    );
}
