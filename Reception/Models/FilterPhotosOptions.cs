using Reception.Models.Entities;

namespace Reception.Models;

public class FilterPhotosOptions
{
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public Dimension? Dimension { get; set; }
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string? Title { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
}
