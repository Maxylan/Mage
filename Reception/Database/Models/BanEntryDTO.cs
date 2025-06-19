using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="BanEntry"/> data transfer object (DTO).
/// </summary>
public class BanEntryDTO : BanEntry, IDataTransferObject<BanEntry>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("client_id")]
    public new int ClientId { get; set; }

    [JsonPropertyName("expires_at")]
    public new DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("reason")]
    public new string? Reason { get; set; }
    */

    /*
    [JsonIgnore, SwaggerIgnore]
    public new Client Client { get; set; } = null!;
    */

    /// <summary>
    /// Convert this <see cref="BanEntryDTO"/> instance to its <see cref="BanEntry"/> equivalent.
    /// </summary>
    public BanEntry ToEntity() => new() {
        Id = this.Id ?? default,
        ClientId = this.ClientId,
        ExpiresAt = this.ExpiresAt,
        Reason = this.Reason,
        // Navigations
        Client = this.Client
    };

    /// <summary>
    /// Compare this <see cref="BanEntryDTO"/> against its <see cref="BanEntry"/> equivalent.
    /// </summary>
    public bool Equals(BanEntry entity) {
        throw new NotImplementedException();
    }
}
