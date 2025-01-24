using Dimension = Reception.Models.Entities.Dimension;
using Filepath = Reception.Models.Entities.Filepath;
using PhotoEntity = Reception.Models.Entities.Photo;
using System.Text.Json.Serialization;

namespace Reception.Models;

/// <summary>
/// <see cref="Reception.Models.Entities.Photo"/> and <see cref="Reception.Models.Entities.Filepath"/> combined, for massive convenience and performance gains.
/// </summary>
public record Photo
{
    public Photo(
        Reception.Models.Entities.Photo entity,
        Dimension dimension
    ) {
        if (entity.Id == default) {
            throw new ArgumentException($"{nameof(Reception.Models.Entities.Photo.Id)} can't be null", nameof(entity.Id));
        }
        if (entity.Filepaths is null || entity.Filepaths.Count == 0) {
            throw new ArgumentException($"Navigation {nameof(Reception.Models.Entities.Photo.Filepaths)} can't be null/empty", nameof(entity));
        }

        var path = entity.Filepaths.FirstOrDefault(path => path.Dimension == dimension);

        if (path is null) {
            throw new ArgumentException($"{nameof(Reception.Models.Entities.Photo)} did not have a {nameof(Reception.Models.Entities.Filepath)} with Dimension {dimension.ToString()}", nameof(entity));
        }
        if (path.PhotoId != entity.Id) {
            throw new ArgumentException($"{nameof(Reception.Models.Entities.Filepath.PhotoId)} ({path.PhotoId}) does not equal {nameof(Reception.Models.Entities.Photo.Id)} ({entity.Id})", nameof(entity.Id));
        }

        _filepath = path;
        _entity = entity;
    }
    
    public Photo(
        Reception.Models.Entities.Photo entity,
        Reception.Models.Entities.Filepath path
    ) {
        if (entity.Id == default) {
            throw new ArgumentException($"{nameof(Reception.Models.Entities.Photo.Id)} can't be null", nameof(entity.Id));
        }
        if (path.PhotoId != entity.Id) {
            throw new ArgumentException($"{nameof(Reception.Models.Entities.Filepath.PhotoId)} ({path.PhotoId}) does not equal {nameof(Reception.Models.Entities.Photo.Id)} ({entity.Id})", nameof(entity.Id));
        }

        _entity = entity;
        _filepath = path;
    }

    public int PhotoId { get => _entity.Id; }
    public int? FilepathId { get => _filepath.Id; }
    public string Slug { get => _entity.Slug; }
    public string? Title { get => _entity.Title; }
    public string? Summary { get => _entity.Summary; }
    public string? Description { get => _entity.Description; }
    public int? CreatedBy { get => _entity.CreatedBy; }
    public DateTime CreatedAt { get => _entity.CreatedAt; }
    public DateTime UpdatedAt { get => _entity.UpdatedAt; }
    public Dimension? Dimension { get => _filepath.Dimension; }
    public long? Filesize { get => _filepath.Filesize; }
    public string Filename { get => _filepath.Filename; }
    public string Path { get => _filepath.Path; }
    public string[] Tags {
        get => _entity.Tags
            .Select(tag => tag.Name)
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
