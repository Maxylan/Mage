using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Account"/> data transfer object (DTO).
/// </summary>
public class AccountDTO : Account, IDataTransferObject<Account>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    /*
    [JsonPropertyName("email")]
    public new string? Email { get; set; }

    [JsonPropertyName("username")]
    public new string Username { get; set; } = null!;

    [JsonPropertyName("password")]
    public new string Password { get; set; } = null!;

    [JsonPropertyName("full_name")]
    public new string? FullName { get; set; }

    [JsonPropertyName("created_at")]
    public new DateTime CreatedAt { get; set; }

    [JsonPropertyName("last_login")]
    public new DateTime LastLogin { get; set; }

    [JsonPropertyName("privilege")]
    public new byte Privilege { get; set; }

    [JsonPropertyName("avatar_id")]
    public new int? AvatarId { get; set; }
    */

    /// <summary>
    /// Convert this <see cref="AccountDTO"/> instance to its <see cref="Account"/> equivalent.
    /// </summary>
    public Account ToEntity() => new() {
        Id = this.Id ?? default,
        Email = this.Email,
        Username = this.Username,
        Password = this.Password,
        FullName = this.FullName,
        CreatedAt = this.CreatedAt,
        LastLogin = this.LastLogin,
        Privilege = this.Privilege,
        AvatarId = this.AvatarId
    };

    /// <summary>
    /// Compare this <see cref="AccountDTO"/> against its <see cref="Account"/> equivalent.
    /// </summary>
    public bool Equals(Account entity) {
        throw new NotImplementedException();
    }
}
