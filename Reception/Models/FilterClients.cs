namespace Reception.Models;

public class FilterClients
{
    /// <summary>
    /// <strong>ID</strong> of the <see cref="Client"/>.
    /// </summary>
    public int? clientId { get; set; }
    /// <summary>
    /// <strong>Address</strong> of the <see cref="Client"/>.
    /// </summary>
    public string? address { get; set; }
    /// <summary>
    /// <strong>User Agent</strong> of the <see cref="Client"/>.
    /// </summary>
    public string? userAgent { get; set; }
    /// <summary>
    /// <strong>Account</strong> (user) <strong>ID</strong> associated with
    /// the <see cref="Client"/>.
    /// </summary>
    public int? accountId { get; set; }
    /// <summary>
    /// <strong>Username</strong> of the <see cref="Account"/> associated with
    /// the <see cref="Client"/>.
    /// </summary>
    public string? username { get; set; }
    /// <summary>
    /// Pagination: Limit
    /// </summary>
    public int? limit { get; set; }
    /// <summary>
    /// Pagination: Offset
    /// </summary>
    public int? offset { get; set; }
}
