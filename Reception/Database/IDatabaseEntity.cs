namespace Reception.Database;

/// <summary>
/// A 'Data Transfer Object' of the <see cref="TEntity"/> db-entity.
/// </summary>
public interface IDatabaseEntity<TEntity> where TEntity : class, new()
{}
