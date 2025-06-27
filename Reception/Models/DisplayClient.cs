using Reception.Database.Models;

namespace Reception.Models;

/// <summary>
/// Collection of all photos (<see cref="Reception.Models.DisplayPhoto"/>) inside the the given
/// <paramref name="album"/> (<see cref="Reception.Database.Models.AlbumDTO"/>).
/// </summary>
public record class DisplayClient
{
    public DisplayClient(ClientDTO client)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(client.Accounts, nameof(client.Accounts));
        if (client.BanEntries is null) {
            client.BanEntries = [];
        }

        Id = client.Id;
        Trusted = client.Trusted;
        Address = client.Address;
        UserAgent = client.UserAgent;
        Logins = client.Logins;
        FailedLogins = client.FailedLogins;
        CreatedAt = client.CreatedAt;
        LastVisit = client.LastVisit;

        this.Accounts = client.Accounts
            .Select(account => (AccountDTO)account);

        this.Bans = client.BanEntries
            .Select(entry => (BanEntryDTO)entry);

        this._isBanned = client.BanEntries
            .Any(entry => entry.ExpiresAt >= DateTime.Now);
    }

    public DisplayClient(Client client)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(client.Accounts, nameof(client.Accounts));
        if (client.BanEntries is null) {
            client.BanEntries = [];
        }

        Id = client.Id;
        Trusted = client.Trusted;
        Address = client.Address;
        UserAgent = client.UserAgent;
        Logins = client.Logins;
        FailedLogins = client.FailedLogins;
        CreatedAt = client.CreatedAt;
        LastVisit = client.LastVisit;

        this.Accounts = client.Accounts
            .Select(account => (AccountDTO)account);

        this.Bans = client.BanEntries
            .Select(entry => (BanEntryDTO)entry);

        this._isBanned = client.BanEntries
            .Any(entry => entry.ExpiresAt >= DateTime.Now);
    }

    public int? Id { get; init; }
    public bool Trusted { get; init; }
    public string Address { get; init; } = null!;
    public string? UserAgent { get; init; }
    public int Logins { get; init; }
    public int FailedLogins { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastVisit { get; init; }

    protected bool _isBanned;
    public bool IsBanned => this._isBanned;

    public readonly IEnumerable<BanEntryDTO> Bans;
    public readonly IEnumerable<AccountDTO> Accounts;
}
