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

    public readonly int? Id;
    public readonly bool Trusted;
    public readonly string Address;
    public readonly string? UserAgent;
    public readonly int Logins;
    public readonly int FailedLogins;
    public readonly DateTime CreatedAt;
    public readonly DateTime LastVisit;

    protected bool _isBanned;
    public bool IsBanned => this._isBanned;

    public readonly IEnumerable<BanEntryDTO> Bans;
    public readonly IEnumerable<AccountDTO> Accounts;
}
