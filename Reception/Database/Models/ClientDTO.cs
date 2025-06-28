using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Client"/> data transfer object (DTO).
/// </summary>
public class ClientDTO : Client, IDataTransferObject<Client>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("trusted")]
    public new bool Trusted { get; set; }

    [JsonPropertyName("address")]
    public new string Address { get; set; } = null!;

    [JsonPropertyName("user_agent")]
    public new string? UserAgent { get; set; }

    [JsonPropertyName("logins")]
    public new int Logins { get; set; }

    [JsonPropertyName("failed_logins")]
    public new int FailedLogins { get; set; }

    [JsonPropertyName("created_at")]
    public new DateTime CreatedAt { get; set; }

    [JsonPropertyName("last_visit")]
    public new DateTime LastVisit { get; set; }
    */

    /*
    [JsonIgnore, SwaggerIgnore]
    public new ICollection<BanEntry> BanEntries { get; set; } = new List<BanEntry>();
    */

    public new ICollection<SessionDTO>? Sessions { get; set; }

    public new ICollection<AccountDTO>? Accounts { get; set; }

    /// <summary>
    /// Convert this <see cref="ClientDTO"/> instance to its <see cref="Client"/> equivalent.
    /// </summary>
    public Client ToEntity() => new() {
        Id = this.Id ?? default,
        Trusted = this.Trusted,
        Address = this.Address,
        UserAgent = this.UserAgent,
        Logins = this.Logins,
        FailedLogins = this.FailedLogins,
        CreatedAt = this.CreatedAt,
        LastVisit = this.LastVisit,
        // Navigations
        BanEntries = this.BanEntries,
        Sessions = this.Sessions!.Select(s => (SessionDTO)s).ToArray(),
        Accounts = this.Accounts!.Select(a => (AccountDTO)a).ToArray()
    };

    /// <summary>
    /// Compare this <see cref="ClientDTO"/> against its <see cref="Client"/> equivalent.
    /// </summary>
    public bool Equals(Client entity) {
        throw new NotImplementedException();
    }
}
