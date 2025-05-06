using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Session"/> db-entity.
/// </summary>
[Table("sessions", Schema = "magedb")]
[Index("AccountId", Name = "idx_sessions_account_id")]
[Index("ClientId", Name = "idx_sessions_client_id")]
[Index("Code", Name = "sessions_code_key", IsUnique = true)]
public partial class Session : IDatabaseEntity<Session>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("account_id")]
    public int AccountId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("code")]
    [StringLength(36)]
    public string Code { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="Session"/> db-entity.
/// </summary>
public partial class Session
{
    [ForeignKey("AccountId")]
    [InverseProperty("Sessions")]
    public virtual Account Account { get; set; } = null!;

    [ForeignKey("ClientId")]
    [InverseProperty("Sessions")]
    public virtual Client Client { get; set; } = null!;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="Session"/>
    /// </summary>
    public static Action<EntityTypeBuilder<Session>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("sessions_pkey");

            entity.Property(e => e.Code).IsFixedLength();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Account).WithMany(p => p.Sessions).HasConstraintName("fk_account");

            entity.HasOne(d => d.Client).WithMany(p => p.Sessions).HasConstraintName("fk_client");
        }
    );
}
