namespace Reception.Models;

public class AlbumSearchQuery
{
    /// <summary>
    /// Album(s) created by <see cref="Account"/> with ID (PK).
    /// </summary>
    public int? CreatedBy { get; set; }
    /// <summary>
    /// Albums created <strong>before</strong> the given date
    /// </summary>
    public DateTime? CreatedBefore { get; set; }
    /// <summary>
    /// Albums created <strong>after</strong> the given date
    /// </summary>
    public DateTime? CreatedAfter { get; set; }
    /// <summary>
    /// Pagination: Limit
    /// </summary>
    public int? Limit { get; set; }
    /// <summary>
    /// Pagination: Offset
    /// </summary>
    public int? Offset { get; set; }
    /// <summary>
    /// Album 'title' to use / filter by.
    /// </summary>
    /// <remarks>
    /// Can also filter / match '<see cref="PhotoEntity"/>' titles when
    /// <seealso cref="AlbumSearchQuery.MatchPhotoTitles"/> is true.
    /// </remarks>
    public string? Title { get; set; }
    /// <summary>
    /// Album 'summary' to use / filter by.
    /// </summary>
    /// <remarks>
    /// Can also filter / match '<see cref="PhotoEntity"/>' summaries when
    /// <seealso cref="AlbumSearchQuery.MatchPhotoSummaries"/> is true.
    /// </remarks>
    public string? Summary { get; set; }
    /// <summary>
    /// Filter / Match the 'title' of contained photos as well?
    /// </summary>
    public bool MatchPhotoTitles { get; set; } = false;
    /// <summary>
    /// Filter / Match the 'summary' of contained photos as well?
    /// </summary>
    public bool MatchPhotoSummaries { get; set; } = false;
    /// <summary>
    /// List of <see cref="Tag"/>s to use/match.
    /// </summary>
    public string[]? Tags { get; set; }
}
