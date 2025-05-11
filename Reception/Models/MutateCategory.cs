using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;
using Reception.Database.Models;

namespace Reception.Models;

public class MutateCategory : CategoryDTO
{
    public new int? Id { get; set; }
    public new IEnumerable<int>? Albums { get; set; }

    /*
    public int Id { get; set; }
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
