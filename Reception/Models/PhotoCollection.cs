using Dimension = Reception.Models.Entities.Dimension;

namespace Reception.Models;

/// <summary>
/// Collection of all different sizes of a <see cref="Reception.Models.Photo"/>.
/// </summary>
public record PhotoCollection
{
    public PhotoCollection(
        Photo source,
        Photo? medium = null,
        Photo? thumbnail = null
    ) {
        if (source.Dimension != Dimension.SOURCE) {
            throw new ArgumentException($"Source Dimension {nameof(Reception.Models.Photo)} didn't match '{Dimension.SOURCE.ToString()}' ({source.Dimension})", nameof(source));
        }
        if (medium is not null && medium.Dimension != Dimension.MEDIUM) {
            throw new ArgumentException($"Medium Dimension {nameof(Reception.Models.Photo)} didn't match '{Dimension.MEDIUM.ToString()}' ({medium.Dimension})", nameof(medium));
        }
        if (thumbnail is not null && thumbnail.Dimension != Dimension.THUMBNAIL) {
            throw new ArgumentException($"Thumbnail Dimension {nameof(Reception.Models.Photo)} didn't match '{Dimension.THUMBNAIL.ToString()}' ({thumbnail.Dimension})", nameof(thumbnail));
        }

        Source = source;
        Medium = medium;
        Thumbnail = thumbnail;
    }
    
    public Photo Source { get; init; }
    public Photo? Medium { get; init; }
    public Photo? Thumbnail { get; init; }
}
