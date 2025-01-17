using Reception.Interfaces;
using Reception.Services;

namespace Reception.Models;

public class Login
{
    /// <summary>
    /// Your <c><see cref="Reception.Models.Entities.Account.Username"/></c>.
    /// </summary>
    public string Username { get; init; } = null!;

    /// <summary>
    /// Your <c><see cref="Reception.Models.Entities.Account.Password"/></c>.
    /// </summary>
    public string Hash { get; init; } = null!;
} 