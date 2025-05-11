using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using Reception.Database.Models;

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
