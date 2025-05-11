namespace Reception.Models;

public struct MutateLink()
{
    public DateTime? ExpiresAt { get; set; } = null;

    public int? AccessLimit { get; set; } = null;
}
