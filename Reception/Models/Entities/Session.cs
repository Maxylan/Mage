using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Models.Entities;

public class Session
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = null!;
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public virtual Account User { get; set; } = null!;

    [JsonIgnore]
    public bool HasUserAgent => !string.IsNullOrWhiteSpace(UserAgent);

    public static Action<EntityTypeBuilder<Session>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("sessions_pkey");

            entity.ToTable("sessions", "magedb");

            entity.HasIndex(e => e.UserId, "idx_sessions_user_id");

            entity.HasIndex(e => e.Code, "sessions_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(36)
                .IsFixedLength()
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(255)
                .HasColumnName("user_agent");

            entity.HasOne(d => d.User).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user");
        }
    );
}
