using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Session"/> data transfer object (DTO).
/// </summary>
public class SessionDTO : Session, IDataTransferObject<Session>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("account_id")]
    public new int AccountId { get; set; }

    [JsonPropertyName("client_id")]
    public new int ClientId { get; set; }

    [JsonPropertyName("code")]
    public new string Code { get; set; } = null!;

    [JsonPropertyName("created_at")]
    public new DateTime CreatedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public new DateTime ExpiresAt { get; set; }
    */

    public new AccountDTO Account { get; set; } = null!;

    public new ClientDTO Client { get; set; } = null!;

    /// <summary>
    /// Convert this <see cref="SessionDTO"/> instance to its <see cref="Session"/> equivalent.
    /// </summary>
    public Session ToEntity() => new() {
        Id = this.Id ?? default,
        AccountId = this.AccountId,
        ClientId = this.ClientId,
        Code = this.Code,
        CreatedAt = this.CreatedAt,
        ExpiresAt = this.ExpiresAt,
        // Navigations
        Account = this.Account,
        Client = this.Client
    };

    /// <summary>
    /// Compare this <see cref="SessionDTO"/> against its <see cref="Session"/> equivalent.
    /// </summary>
    public bool Equals(Session entity) {
        throw new NotImplementedException();
    }
}
