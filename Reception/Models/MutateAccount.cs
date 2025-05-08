using System.Text.Json.Serialization;
using Reception.Database.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Models;

public class MutateAccount : AccountDTO
{
    [JsonIgnore, SwaggerIgnore]
    public new string? Password { get; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? CreatedAt { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? LastLogin { get; set; }
}
