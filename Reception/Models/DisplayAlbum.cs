using Reception.Database.Models;

namespace Reception.Models;

/// <summary>
/// Collection of all photos (<see cref="Reception.Models.DisplayPhoto"/>) inside the the given
/// <paramref name="album"/> (<see cref="Reception.Database.Models.AlbumDTO"/>).
/// </summary>
public record class DisplayAlbum
{
    public DisplayAlbum(AlbumDTO album, int? currentUserId = null)
    {
        ArgumentNullException.ThrowIfNull(album, nameof(album));
        ArgumentNullException.ThrowIfNull(album.Photos, nameof(album.Photos));

        AlbumId = album.Id;

        ThumbnailId = album.ThumbnailId;
        if (album.ThumbnailId > 0 && album.Thumbnail is not null) {
            Thumbnail = new DisplayPhoto(album.Thumbnail);
        }

        CategoryId = album.CategoryId;
        if (album.CategoryId > 0 && album.Category is not null) {
            Category = (CategoryDTO)album.Category;
        }

        this._favoritedBy = album.FavoritedBy;
        this._currentUserId = currentUserId;

        Title = album.Title;
        Summary = album.Summary;
        Description = album.Description;

        this._tags = album.Tags
            .Select(t => (TagDTO)t.Tag);

        CreatedAt = album.CreatedAt;
        UpdatedAt = album.UpdatedAt;
        RequiredPrivilege = (byte)album.RequiredPrivilege;

        this._photos = album.Photos
            .Where(p => p.Photo is not null && p.Photo.Filepaths.Any())
            .OrderByDescending(p => p.Added)
            .Select(p => new DisplayPhoto(p.Photo));

        this.UpdatedByUserId = album.UpdatedBy;
        var updatedBy = album.UpdatedByNavigation;
        if (updatedBy is not null) {
            this._updatedBy = (AccountDTO)updatedBy;
        }

        this.CreatedByUserId = album.CreatedBy;
        var createdBy = album.CreatedByNavigation;
        if (createdBy is not null) {
            this._createdBy = (AccountDTO)createdBy;
        }
    }

    public DisplayAlbum(Album album, int? currentUserId = null)
    {
        ArgumentNullException.ThrowIfNull(album, nameof(album));
        ArgumentNullException.ThrowIfNull(album.Photos, nameof(album.Photos));

        AlbumId = album.Id;

        ThumbnailId = album.ThumbnailId;
        if (album.ThumbnailId > 0 && album.Thumbnail is not null) {
            Thumbnail = new DisplayPhoto(album.Thumbnail);
        }

        CategoryId = album.CategoryId;
        if (album.CategoryId > 0 && album.Category is not null) {
            Category = (CategoryDTO)album.Category;
        }

        this._favoritedBy = album.FavoritedBy;
        this._currentUserId = currentUserId;

        Title = album.Title;
        Summary = album.Summary;
        Description = album.Description;

        this._tags = album.Tags
            .Select(t => (TagDTO)t.Tag);

        CreatedAt = album.CreatedAt;
        UpdatedAt = album.UpdatedAt;
        RequiredPrivilege = (byte)album.RequiredPrivilege;

        this._photos = album.Photos
            .Where(p => p.Photo is not null && p.Photo.Filepaths.Any())
            .OrderByDescending(p => p.Added)
            .Select(p => new DisplayPhoto(p.Photo));

        this.UpdatedByUserId = album.UpdatedBy;
        var updatedBy = album.UpdatedByNavigation;
        if (updatedBy is not null) {
            this._updatedBy = (AccountDTO)updatedBy;
        }

        this.CreatedByUserId = album.CreatedBy;
        var createdBy = album.CreatedByNavigation;
        if (createdBy is not null) {
            this._createdBy = (AccountDTO)createdBy;
        }
    }

    protected IEnumerable<DisplayPhoto> _photos;
    public IEnumerable<DisplayPhoto> Photos {
        get => _photos;
    }

    /// <summary>
    /// Returns the number of elements (photos) in this sequence.
    /// </summary>
    public int Count => this._photos.Count();

    protected int? _currentUserId;
    protected IEnumerable<FavoriteAlbumRelation> _favoritedBy;

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

    public int? AlbumId { get; init; }
    public int? ThumbnailId { get; init; }
    public DisplayPhoto? Thumbnail { get; init; }
    public int? CategoryId { get; init; }
    public CategoryDTO? Category { get; init; }
    public string Title { get; init; } = null!;
    public string? Summary { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public byte RequiredPrivilege { get; init; }

    protected IEnumerable<TagDTO> _tags;
    public IEnumerable<TagDTO> Tags => this._tags;

    protected AccountDTO? _updatedBy;

    public readonly int? UpdatedByUserId;
    public AccountDTO? UpdatedByUser => this._updatedBy;

    protected AccountDTO? _createdBy;

    public readonly int? CreatedByUserId;
    public AccountDTO? CreatedByUser => this._createdBy;
}
