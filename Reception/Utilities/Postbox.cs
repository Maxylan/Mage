using Reception.Database;

namespace Reception.Utilities;

public static class Postbox
{
    #region Get base filepaths.
    public const string FILE_STORAGE_NAME = "Postbox";
    public static readonly Dictionary<Dimension, string> StorageDirectories = new() {
        { Dimension.SOURCE, "source" },
        { Dimension.MEDIUM, "medium" },
        { Dimension.THUMBNAIL, "thumbnail" }
    };

    /// <summary>
    /// Get the name (only) of the base directory of my file storage
    /// </summary>
    public static string GetBaseDirectoryName() => FILE_STORAGE_NAME;
    /// <summary>
    /// Get the name (only) of the Thumbnail directory of my file storage
    /// </summary>
    public static string GetThumbnailDirectoryName() => StorageDirectories[Dimension.THUMBNAIL];
    /// <summary>
    /// Get the name (only) of the Medium directory of my file storage
    /// </summary>
    public static string GetMediumDirectoryName() => StorageDirectories[Dimension.MEDIUM];
    /// <summary>
    /// Get the name (only) of the Source directory of my file storage
    /// </summary>
    public static string GetSourceDirectoryName() => StorageDirectories[Dimension.SOURCE];
    /// <summary>
    /// Get the path (directories, plural) to the directory relative to a <see cref="DateTime"/>
    /// </summary>
    public static string GetDatePath(DateTime dateTime) => Path.Combine(
        dateTime.Year.ToString(),
        dateTime.Month.ToString(),
        dateTime.Day.ToString()
    );
    /// <summary>
    /// Get the <strong>combined</strong> relative path (<c>Base + Thumbnail/Medium/Source + DatePath</c>) to a directory in my file storage.
    /// </summary>
    public static string GetCombinedPath(Dimension dimension, DateTime? dateTime = null, string filename = "") => Path.Combine(
        GetBaseDirectoryName(),
        dimension switch
        {
            Dimension.THUMBNAIL => GetThumbnailDirectoryName(),
            Dimension.MEDIUM => GetMediumDirectoryName(),
            Dimension.SOURCE => GetSourceDirectoryName(),
            _ => GetSourceDirectoryName()
        },
        GetDatePath(dateTime ?? DateTime.UtcNow.ToServerTime()),
        filename
    );
    #endregion
}
