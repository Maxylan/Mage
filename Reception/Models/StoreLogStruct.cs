using Reception.Interfaces;
using Reception.Services;

namespace Reception.Models;

public readonly struct StoreLogsInDatabase(Func<MageDbContext> getDbContext)
{
    /// <summary>
    /// Calls <c><see cref="MageDbContext.SaveChangesAsync"/></c> to asynchronously store your newly created logs.
    /// </summary>
    /// <remarks>
    /// <c><seealso cref="Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync"/></c>
    /// </remarks>
    public async Task<int> SaveAsync() => await getDbContext().SaveChangesAsync();

    /// <summary>
    /// Calls <c><see cref="MageDbContext.SaveChanges"/></c> to store your newly created logs.
    /// </summary>
    /// <remarks>
    /// <c><seealso cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges"/></c>
    /// </remarks>
    public int Save() => getDbContext().SaveChanges();
} 