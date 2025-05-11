using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;
using Reception.Database.Models;

namespace Reception.Models;

public class MutateAlbum : AlbumDTO
{
    public new int? Id { get; set; }
    public new IEnumerable<MutateTag>? Tags { get; set; }
    public new IEnumerable<int>? Photos { get; set; }

    /*
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public int? ThumbnailId { get; set; }
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    */

    [JsonIgnore, SwaggerIgnore]
    public new int? CreatedBy { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new int? UpdatedBy { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? CreatedAt { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? UpdatedAt { get; set; }
}
