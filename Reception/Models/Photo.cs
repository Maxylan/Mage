using System.Text.Json.Serialization;
using Reception.Database.Models;
using Reception.Services;

namespace Reception.Models;

/// <summary>
/// <see cref="PhotoEntity"/> and <see cref="Filepath"/> combined, for massive convenience and performance gains.
/// </summary>
public record Photo
{
    public Photo(
        PhotoEntity entity,
        Dimension dimension,
        bool defaultToSource = false
    )
    {
        if (entity.Id == default)
        {
            throw new ArgumentException($"{nameof(PhotoEntity.Id)} can't be null", nameof(entity.Id));
        }
        if (entity.Filepaths is null || entity.Filepaths.Count == 0)
        {
            throw new ArgumentException($"Navigation {nameof(PhotoEntity.Filepaths)} can't be null/empty", nameof(entity));
        }

        var path = entity.Filepaths
            .FirstOrDefault(path => path.Dimension == dimension);

        if (path is null)
        {
            if (defaultToSource)
            {
                path = entity.Filepaths
                    .FirstOrDefault(path => path.Dimension == Reception.Models.Entities.Dimension.SOURCE);
            }

            if (path is null) {
                throw new ArgumentException($"{nameof(PhotoEntity)} did not have a {nameof(Filepath)} with Dimension {dimension.ToString()}", nameof(entity));
            }
        }
        if (path.PhotoId != entity.Id)
        {
            throw new ArgumentException($"{nameof(Filepath.PhotoId)} ({path.PhotoId}) does not equal {nameof(PhotoEntity.Id)} ({entity.Id})", nameof(entity.Id));
        }

        _filepath = path;
        _entity = entity;
    }

    public Photo(
        PhotoEntity entity,
        Filepath path
    )
    {
        if (entity.Id == default)
        {
            throw new ArgumentException($"{nameof(PhotoEntity.Id)} can't be null", nameof(entity.Id));
        }
        if (path.PhotoId != entity.Id)
        {
            throw new ArgumentException($"{nameof(Filepath.PhotoId)} ({path.PhotoId}) does not equal {nameof(PhotoEntity.Id)} ({entity.Id})", nameof(entity.Id));
        }

        _entity = entity;
        _filepath = path;
    }

    public int PhotoId { get => _entity.Id; }
    public int? FilepathId { get => _filepath.Id; }
    public string Slug { get => _entity.Slug; }
    public string Title { get => _entity.Title; }
    public string? Summary { get => _entity.Summary; }
    public string? Description { get => _entity.Description; }
    public int? UploadedBy { get => _entity.UploadedBy; }
    public DateTime UploadedAt { get => _entity.UploadedAt; }
    public DateTime CreatedAt { get => _entity.CreatedAt; }
    public DateTime UpdatedAt { get => _entity.UpdatedAt; }
    public Dimension? Dimension { get => _filepath.Dimension; }
    public long? Filesize { get => _filepath.Filesize; }
    public int? Height { get => _filepath.Height; }
    public int? Width { get => _filepath.Width; }
    public string Filename { get => _filepath.Filename; }
    public string Path { get => _filepath.Path; }
    public string[] Tags
    {
        get => _entity.Tags
            .Select(tag => tag.Name)
            .ToArray();
    }
    public string[] Links
    {
        get => _entity.Links
            .Select(link => LinkService.GenerateLinkUri(link.Code, this.Dimension).ToString())
            .ToArray();
    }


    [JsonIgnore]
    protected PhotoEntity _entity;

    [JsonIgnore]
    public PhotoEntity Entity { get => _entity; }

    [JsonIgnore]
    protected Filepath _filepath;

    [JsonIgnore]
    public Filepath Filepath { get => _filepath; }
}
