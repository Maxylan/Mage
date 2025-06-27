using System.Text.Json.Serialization;
using Reception.Database;
using Reception.Database.Models;

namespace Reception.Models;

/// <summary>
/// Collection of all photos (<see cref="Reception.Models.DisplayPhoto"/>) inside the the given
/// <paramref name="album"/> (<see cref="Reception.Database.Models.AlbumDTO"/>).
/// </summary>
public record class DisplayPhoto
{
    public DisplayPhoto(PhotoDTO photo, int? currentUserId = null)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(photo));
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(photo.Filepaths));
        if (!photo.Filepaths.Any(path => path.IsSource)) {
            throw new ArgumentException($"{nameof(photo.Filepaths)} has to contain a source image filepath!", nameof(photo.Filepaths));
        }

        PhotoId = photo.Id;
        Slug = photo.Slug;
        Title = photo.Title;
        Summary = photo.Summary;
        Description = photo.Description;
        UploadedAt = photo.UploadedAt;
        UpdatedAt = photo.UpdatedAt;
        CreatedAt = photo.CreatedAt;
        IsAnalyzed = photo.IsAnalyzed;
        AnalyzedAt = photo.AnalyzedAt;
        RequiredPrivilege = (byte)photo.RequiredPrivilege;

        this._favoritedBy = photo.FavoritedBy;
        this._currentUserId = currentUserId;
        this._filepaths = photo.Filepaths
            .Select(path => (FilepathDTO)path);

        this._publicLinks = photo.PublicLinks
            .Select(pl => (PublicLinkDTO)pl);

        this._albums = photo.Albums
            .Select(pa => (PhotoAlbumRelationDTO)pa);

        this._tags = photo.Tags
            .Select(t => (TagDTO)t.Tag);

        this.UpdatedByUserId = photo.UpdatedBy;
        var updatedBy = photo.UpdatedByNavigation;
        if (updatedBy is not null) {
            this._updatedBy = (AccountDTO)updatedBy;
        }

        this.UploadedByUserId = photo.UploadedBy;
        var uploadedBy = photo.UploadedByNavigation;
        if (uploadedBy is not null) {
            this._uploadedBy = (AccountDTO)uploadedBy;
        }
    }

    public DisplayPhoto(Photo photo, int? currentUserId = null)
    {
        ArgumentNullException.ThrowIfNull(photo, nameof(photo));
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(photo.Filepaths));
        if (!photo.Filepaths.Any(path => path.IsSource)) {
            throw new ArgumentException($"{nameof(photo.Filepaths)} has to contain a source image filepath!", nameof(photo.Filepaths));
        }

        PhotoId = photo.Id;
        Slug = photo.Slug;
        Title = photo.Title;
        Summary = photo.Summary;
        Description = photo.Description;
        UploadedAt = photo.UploadedAt;
        UpdatedAt = photo.UpdatedAt;
        CreatedAt = photo.CreatedAt;
        IsAnalyzed = photo.IsAnalyzed;
        AnalyzedAt = photo.AnalyzedAt;
        RequiredPrivilege = (byte)photo.RequiredPrivilege;

        this._favoritedBy = photo.FavoritedBy;
        this._currentUserId = currentUserId;
        this._filepaths = photo.Filepaths
            .Select(path => (FilepathDTO)path);

        this._publicLinks = photo.PublicLinks
            .Select(pl => (PublicLinkDTO)pl);

        this._albums = photo.Albums
            .Select(pa => (PhotoAlbumRelationDTO)pa);

        this._tags = photo.Tags
            .Select(t => (TagDTO)t.Tag);

        this.UpdatedByUserId = photo.UpdatedBy;
        var updatedBy = photo.UpdatedByNavigation;
        if (updatedBy is not null) {
            this._updatedBy = (AccountDTO)updatedBy;
        }

        this.UploadedByUserId = photo.UploadedBy;
        var uploadedBy = photo.UploadedByNavigation;
        if (uploadedBy is not null) {
            this._uploadedBy = (AccountDTO)uploadedBy;
        }
    }

    public int? PhotoId { get; init; }
    public string Slug { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string? Summary { get; init; }
    public string? Description { get; init; }
    public DateTime UploadedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsAnalyzed { get; init; }
    public DateTime? AnalyzedAt { get; init; }
    public byte RequiredPrivilege { get; init; }

    protected int? _currentUserId;
    protected IEnumerable<FavoritePhotoRelation> _favoritedBy;

    /// <summary>
    /// Returns the number of times this photo has been favorited.
    /// </summary>
    public int Favorites => this._favoritedBy.Count();

    /// <summary>
    /// Returns a flag indicating if this has been favorited by you (current user).
    /// </summary>
    public bool IsFavorite =>
        _currentUserId is not null &&
        _currentUserId > 0 &&
        this._favoritedBy.Any(f => f.AccountId == _currentUserId);

    protected IEnumerable<FilepathDTO> _filepaths;

    public FilepathDTO Source => this._filepaths.First(path => path.Dimension == Dimension.SOURCE);
    public FilepathDTO? Medium => this._filepaths.FirstOrDefault(path => path.Dimension == Dimension.MEDIUM);
    public FilepathDTO? Thumbnail => this._filepaths.FirstOrDefault(path => path.Dimension == Dimension.THUMBNAIL);

    public bool HasMedium => this.Medium is not null;
    public bool HasThumbnail => this.Thumbnail is not null;

    protected IEnumerable<PublicLinkDTO> _publicLinks;
    public IEnumerable<PublicLinkDTO> PublicLinks => this._publicLinks;

    protected IEnumerable<PhotoAlbumRelationDTO> _albums;
    public IEnumerable<PhotoAlbumRelationDTO> RelatedAlbums => this._albums;

    protected IEnumerable<TagDTO> _tags;
    public IEnumerable<TagDTO> Tags => this._tags;

    protected AccountDTO? _updatedBy;

    public readonly int? UpdatedByUserId;
    public AccountDTO? UpdatedByUser => this._updatedBy;

    protected AccountDTO? _uploadedBy;

    public readonly int? UploadedByUserId;
    public AccountDTO? UploadedByUser => this._uploadedBy;
}
