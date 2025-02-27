using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Models.Entities;

public class Account
{
    public int Id { get; set; }

    public string? Email { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastVisit { get; set; }

    public short Permissions { get; set; }

    // Navigation Properties

    [JsonIgnore]
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    [JsonIgnore]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [JsonIgnore]
    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();

    [JsonIgnore]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public static Action<EntityTypeBuilder<Account>> Build => (
        entity => {
            entity.HasKey(e => e.Id).HasName("accounts_pkey");

            entity.ToTable("accounts", "magedb");

            entity.HasIndex(e => e.Email, "accounts_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "accounts_username_key").IsUnique();

            entity.HasIndex(e => e.Email, "idx_accounts_email").IsUnique();

            entity.HasIndex(e => e.LastVisit, "idx_accounts_last_visit");

            entity.HasIndex(e => e.Username, "idx_accounts_username").IsUnique();

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
        }
    );
}
