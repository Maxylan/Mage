namespace Reception.Database;

/// <summary>
/// A 'Data Transfer Object' of the <see cref="TEntity"/> db-entity.
/// </summary>
public interface IDataTransferObject<TEntity> where TEntity : class, IDatabaseEntity<TEntity>, new()
{
    /// <summary>
    /// Convert a <see cref="IDataTransferObject"/> instance to its <see cref="IDatabaseEntity{TEntity}"/> equivalent.
    /// </summary>
    public abstract TEntity ToEntity();

    /// <summary>
    /// Compare an <see cref="IDataTransferObject"/> against its <see cref="IDatabaseEntity{TEntity}"/> equivalent.
    /// </summary>
    public abstract bool Equals(TEntity entity);
}
