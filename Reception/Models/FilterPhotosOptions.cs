using Reception.Database;

namespace Reception.Models;

public class FilterPhotosOptions : PhotosOptions
{
    /// <summary>
    /// Images uploaded <strong>before</strong> the given date
    /// </summary>
    public DateTime? UploadedBefore { get; set; }
    /// <summary>
    /// Images uploaded <strong>after</strong> the given date
    /// </summary>
    public DateTime? UploadedAfter { get; set; }
    /// <summary>
    /// Images taken/created <strong>before</strong> the given date
    /// </summary>
    public DateTime? CreatedBefore { get; set; }
    /// <summary>
    /// Images taken/created <strong>after</strong> the given date
    /// </summary>
    public DateTime? CreatedAfter { get; set; }
}

public class PhotosOptions
{
    /// <summary>
    /// Pagination: Limit
    /// </summary>
    public int? Limit { get; set; }
    /// <summary>
    /// Pagination: Offset
    /// </summary>
    public int? Offset { get; set; }
    /// <summary>
    /// The <see cref="Dimension"/> to use / filter by.
    /// </summary>
    public Dimension? Dimension { get; set; }
    /// <summary>
    /// Unique 'slug' to use / filter by.
    /// </summary>
    public string? Slug { get; set; }
    /// <summary>
    /// Photo 'summary' to use / filter by.
    /// </summary>
    public string? Summary { get; set; }
    /// <summary>
    /// Photo 'title' to use / filter by.
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// Photo(s) uploaded by <see cref="Account"/> with ID (PK).
    /// </summary>
    public int? UploadedBy { get; set; }
    /// <summary>
    /// List of <see cref="Tag"/>s to use/match.
    /// </summary>
    public string[]? Tags { get; set; }
}
