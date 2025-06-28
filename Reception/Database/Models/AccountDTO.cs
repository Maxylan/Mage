using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Account"/> data transfer object (DTO).
/// </summary>
public class AccountDTO : Account, IDataTransferObject<Account>
{
    [JsonPropertyName("id")]
    public new int? Id { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new string? Password { get; set; }

    /*
    [JsonPropertyName("email")]
    public new string? Email { get; set; }

    [JsonPropertyName("username")]
    public new string Username { get; set; } = null!;

    [JsonPropertyName("full_name")]
    public new string? FullName { get; set; }

    [JsonPropertyName("created_at")]
    public new DateTime CreatedAt { get; set; }

    [JsonPropertyName("last_login")]
    public new DateTime LastLogin { get; set; }

    [JsonPropertyName("privilege")]
    public new byte Privilege { get; set; }

    [JsonPropertyName("avatar_id")]
    public new int? AvatarId { get; set; }
    */

    /*
    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Album> AlbumsCreated { get; set; } = new List<Album>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Album> AlbumsUpdated { get; set; } = new List<Album>();
    */

    public new PhotoDTO? Avatar { get; set; }

    /*
    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Category> CreatedCategories { get; set; } = new List<Category>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Category> UpdatedCategories { get; set; } = new List<Category>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<FavoriteAlbumRelation> FavoriteAlbums { get; set; } = new List<FavoriteAlbumRelation>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<FavoritePhotoRelation> FavoritePhotos { get; set; } = new List<FavoritePhotoRelation>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<PublicLink> LinksCreated { get; set; } = new List<PublicLink>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Photo> PhotosUpdated { get; set; } = new List<Photo>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Photo> PhotosUploaded { get; set; } = new List<Photo>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Session> Sessions { get; set; } = new List<Session>();

    [JsonIgnore, SwaggerIgnore]
    public new ICollection<Client> Clients { get; set; } = new List<Client>();
    */

    /// <summary>
    /// Convert this <see cref="AccountDTO"/> instance to its <see cref="Account"/> equivalent.
    /// </summary>
    public Account ToEntity() => new() {
        Id = this.Id ?? default,
        Email = this.Email,
        Username = this.Username,
        Password = this.Password,
        FullName = this.FullName,
        CreatedAt = this.CreatedAt,
        LastLogin = this.LastLogin,
        Privilege = this.Privilege,
        AvatarId = this.AvatarId,
        // Navigations
        AlbumsCreated = this.AlbumsCreated,
        AlbumsUpdated = this.AlbumsUpdated,
        Avatar = this.Avatar,
        CreatedCategories = this.CreatedCategories,
        UpdatedCategories = this.UpdatedCategories,
        FavoriteAlbums = this.FavoriteAlbums,
        FavoritePhotos = this.FavoritePhotos,
        LinksCreated = this.LinksCreated,
        PhotosUpdated = this.PhotosUpdated,
        PhotosUploaded = this.PhotosUploaded,
        Sessions = this.Sessions,
        Clients = this.Clients
    };

    /// <summary>
    /// Compare this <see cref="AccountDTO"/> against its <see cref="Account"/> equivalent.
    /// </summary>
    public bool Equals(Account entity) {
        throw new NotImplementedException();
    }
}
