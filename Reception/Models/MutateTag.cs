using System.Text.Json.Serialization;
using Reception.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Models;

public class MutateTag : Tag
{
    [JsonIgnore, SwaggerIgnore]
    public new int? Id { get; set; }

    /*
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    */
}
