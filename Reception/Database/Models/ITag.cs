namespace Reception.Database.Models;

/// <summary>
/// Base of the <see cref="Tag"/> db-entity, to make internal API's easier to use.
/// </summary>
public interface ITag
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public byte RequiredPrivilege { get; set; }
}
