using System.Text.Json.Serialization;
using Reception.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Reception.Models;

public class MutateAccount : Account
{
    [JsonIgnore, SwaggerIgnore]
    public new string? Password { get; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? CreatedAt { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public new DateTime? LastVisit { get; set; }
}
