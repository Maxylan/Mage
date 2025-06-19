using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Client"/> db-entity.
/// </summary>
[Table("clients", Schema = "magedb")]
[Index("LastVisit", Name = "idx_clients_last_visit")]
public partial class Client : IDatabaseEntity<Client>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("trusted")]
    public bool Trusted { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string Address { get; set; } = null!;

    [Column("user_agent")]
    [StringLength(1023)]
    public string? UserAgent { get; set; }

    [Column("logins")]
    public int Logins { get; set; }

    [Column("failed_logins")]
    public int FailedLogins { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_visit")]
    public DateTime LastVisit { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="Client"/> db-entity.
/// </summary>
public partial class Client
{
    [InverseProperty("Client")]
    public virtual ICollection<BanEntry> BanEntries { get; set; } = new List<BanEntry>();

    [InverseProperty("Client")]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    [ForeignKey("ClientId")]
    [InverseProperty("Clients")]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="Client"/>
    /// </summary>
    public static Action<EntityTypeBuilder<Client>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("clients_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FailedLogins).HasDefaultValue(0);
            entity.Property(e => e.LastVisit).HasDefaultValueSql("now()");
            entity.Property(e => e.Logins).HasDefaultValue(0);
            entity.Property(e => e.Trusted).HasDefaultValue(false);
        }
    );
}
