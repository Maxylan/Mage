/*
using System.Diagnostics.CodeAnalysis;
using Reception.Database.Models;
*/

// namespace Reception.Models;

/// <summary>
/// Collection of all different sizes of a <see cref="Reception.Models.Photo"/>.
/// </summary>
/*
public record PhotoCollection
{
    [SetsRequiredMembers]
    public PhotoCollection(
        PhotoEntity entity
    )
    {
        foreach (var filepath in entity.Filepaths)
        {
            switch (filepath.Dimension)
            {
                case Dimension.SOURCE:
                    Source = new Photo(entity, filepath);
                    break;
                case Dimension.MEDIUM:
                    Medium = new Photo(entity, filepath);
                    break;
                case Dimension.THUMBNAIL:
                    Thumbnail = new Photo(entity, filepath);
                    break;
            }
        }

        if (Source is null)
        {
            throw new ArgumentException($"{nameof(PhotoEntity)} didn't have a '{Dimension.SOURCE}' {nameof(Dimension)}", nameof(entity));
        }
    }

    [SetsRequiredMembers]
    public PhotoCollection(
        Photo source,
        Photo? medium = null,
        Photo? thumbnail = null
    )
    {
        if (source.Dimension != Dimension.SOURCE)
        {
            throw new ArgumentException($"Source Dimension {nameof(Reception.Models.Photo)} didn't match '{Dimension.SOURCE}' ({source.Dimension})", nameof(source));
        }
        if (medium is not null && medium.Dimension != Dimension.MEDIUM)
        {
            throw new ArgumentException($"Medium Dimension {nameof(Reception.Models.Photo)} didn't match '{Dimension.MEDIUM}' ({medium.Dimension})", nameof(medium));
        }
        if (thumbnail is not null && thumbnail.Dimension != Dimension.THUMBNAIL)
        {
            throw new ArgumentException($"Thumbnail Dimension {nameof(Reception.Models.Photo)} didn't match '{Dimension.THUMBNAIL}' ({thumbnail.Dimension})", nameof(thumbnail));
        }

        Source = source;
        Medium = medium;
        Thumbnail = thumbnail;
    }

    public int PhotoId { get => Source.PhotoId; }

    // // Unsure about repeating these two here..
    // public string Slug { get => Source.Slug; }
    // public string Title { get => Source.Title; }

    public required Photo Source { get; init; }
    public Photo? Medium { get; init; }
    public Photo? Thumbnail { get; init; }
}
*/
