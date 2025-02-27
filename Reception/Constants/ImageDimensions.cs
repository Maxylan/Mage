namespace Reception.Constants;

/// <summary>
/// Image Constants.
/// </summary>
public static class ImageDimensions
{
    /// <summary>
    /// Image size limits
    /// </summary>
    public static class Limits
    {
        public const int MIN_WIDTH = 4;
        public const int MIN_HEIGHT = 4;
        public const int MAX_WIDTH = 32768;
        public const int MAX_HEIGHT = 32768;
    }

    /// <summary>
    /// Medium Dimensions
    /// </summary>
    public static class Medium
    {
        public const int TARGET = 640;
        public const int CLAMP_MINIMUM = 480;
        public const int CLAMP_MAXIMUM = 800;
    }

    /// <summary>
    /// Thumbnail Dimensions
    /// </summary>
    public static class Thumbnail
    {
        public const int TARGET = 128;
        public const int CLAMP_MINIMUM = 64;
        public const int CLAMP_MAXIMUM = 256;
    }
}
