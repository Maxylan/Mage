using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="BanEntry"/> db-entity.
/// </summary>
[Table("banned_clients", Schema = "magedb")]
[Index("ClientId", Name = "idx_banned_clients_client_id")]
public partial class BanEntry : IDatabaseEntity<BanEntry>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="BanEntry"/> db-entity.
/// </summary>
public partial class BanEntry
{
    [ForeignKey("ClientId")]
    [InverseProperty("BanEntries")]
    public virtual Client Client { get; set; } = null!;

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="BanEntry"/>
    /// </summary>
    public static Action<EntityTypeBuilder<BanEntry>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("banned_clients_pkey");

            entity.HasOne(d => d.Client).WithMany(p => p.BanEntries).HasConstraintName("fk_client");
        }
    );
}
