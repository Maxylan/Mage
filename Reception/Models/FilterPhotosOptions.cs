using Reception.Models.Entities;

namespace Reception.Models;

public class FilterPhotosOptions
{
    int? Limit { get; set; }
    int? Offset { get; set; }
    Dimension? Dimension { get; set; }
    string? Slug { get; set; }
    string? Title { get; set; }
    string? FileName { get; set; }
    DateTime? CreatedAt { get; set; }
    int? CreatedBy { get; set; }
}
