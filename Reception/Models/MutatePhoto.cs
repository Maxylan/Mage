using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;
using Reception.Database.Models;

namespace Reception.Models;

public class MutatePhoto : PhotoDTO
{
    public new int? Id { get; set; }
    public new IEnumerable<ITag>? Tags { get; set; }
    public new IEnumerable<int>? Albums { get; set; }

    /*
    public string Slug { get; set; } = null!;
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    */

    [JsonIgnore, SwaggerIgnore]
    public new int? UploadedBy { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new int? UpdatedBy { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? UploadedAt { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? UpdatedAt { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? CreatedAt { get; set; }
}
