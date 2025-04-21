using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;
using Reception.Models.Entities;

namespace Reception.Models;

public class MutatePhoto : PhotoEntity
{
    public new int? Id { get; set; }
    public new Tag[]? Tags { get; set; }

    /*
    public string Slug { get; set; } = null!;
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    */

    [JsonIgnore, SwaggerIgnore]
    public new int? UploadedBy { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? UploadedAt { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? UpdatedAt { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? CreatedAt { get; set; }
}
