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
[Table("sessions", Schema = "magedb")]
[Index("UserId", Name = "idx_sessions_user_id")]
[Index("Code", Name = "sessions_code_key", IsUnique = true)]
*/
/*
public class Session
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(36)]
    public string Code { get; set; } = null!;

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    // Methods
    [JsonIgnore, SwaggerIgnore]
    public bool HasUserAgent => !string.IsNullOrWhiteSpace(UserAgent);

    // Navigation Properties

    [JsonIgnore, SwaggerIgnore]
    [ForeignKey("UserId")]
    [InverseProperty("Sessions")]
    public virtual Account User { get; set; } = null!;

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
*/
