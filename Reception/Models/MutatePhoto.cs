using System.Text.Json.Serialization;
using Reception.Models.Entities;

namespace Reception.Models;

public class MutatePhoto : PhotoEntity
{
    public new int? Id { get; set; }
    
    /* 
    public string Name { get; set; } = null!;
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    */

    [JsonIgnore]
    public new int? CreatedBy { get; set; }

    [JsonIgnore]
    public new DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public new DateTime UpdatedAt { get; set; }
}
