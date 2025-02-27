namespace Reception.Utilities;

public static class StringExtensions
{
    /// <summary>
    /// How '<see cref="string.Substring(int)"/>' *should* work..
    /// <para>
    ///     Retrieves a substring from this instance. The substring starts at a specified character position and continues to the end of the string.
    /// </para>
    /// </summary>
    /// <remarks>
    /// XML-Doc comment/description taken straight from '<seealso cref="string.Substring(int)"/>'
    /// </remarks>
    /// <returns>
    ///     A string that is equivalent to the substring that begins at <paramref name="startIndex"/> in this instance, or <c>string.Empty</c> if
    ///     <paramref name="startIndex"/> is equal to the length of this instance.
    /// </returns>
    public static string Subsmart(this string str, int startIndex)
    {
        if (string.IsNullOrEmpty(str)) {
            return str;
        }

        if (startIndex >= str.Length) {
            return string.Empty;
        }
        else if (startIndex < 0) {
            startIndex = 0;
        }

        return str.Substring(startIndex);
    }
    /// <summary>
    /// How '<see cref="string.Substring(int)"/>' *should* work..
    /// <para>
    ///     Retrieves a substring from this instance. The substring starts at a specified character position and continues to the end of the string.
    /// </para>
    /// </summary>
    /// <remarks>
    /// XML-Doc comment/description taken straight from '<seealso cref="string.Substring(int)"/>'
    /// </remarks>
    /// <returns>
    ///     A string that is equivalent to the substring that begins at <paramref name="startIndex"/> in this instance, or <c>string.Empty</c> if
    ///     <paramref name="startIndex"/> is equal to the length of this instance.
    /// </returns>
    public static string? Subsmarter(this string? str, int startIndex)
    {
        if (string.IsNullOrEmpty(str)) {
            return str;
        }

        if (startIndex >= str.Length) {
            return string.Empty;
        }
        else if (startIndex < 0) {
            startIndex = 0;
        }

        return str.Substring(startIndex);
    }
    /// <summary>
    /// How '<see cref="string.Substring(int, int)"/>' *should* work..
    /// <para>
    ///     Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.
    /// </para>
    /// </summary>
    /// <remarks>
    /// XML-Doc comment/description taken straight from '<seealso cref="string.Substring(int, int)"/>'
    /// </remarks>
    /// <returns>
    ///     A string that is equivalent to the substring of <paramref name="maxLength"/> length that begins at startIndex in this instance, or
    ///     <c>string.Empty</c> if <paramref name="startIndex"/> is equal to the length of this instance and length is zero.
    /// </returns>
    public static string Subsmart(this string str, int startIndex, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) {
            return str;
        }

        if (startIndex < 0) {
            startIndex = 0;
        }
        if (startIndex + maxLength >= str.Length) {
            return str.Substring(startIndex);
        }

        return str.Substring(startIndex, maxLength);
    }
    /// <summary>
    /// How '<see cref="string.Substring(int, int)"/>' *should* work..
    /// <para>
    ///     Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.
    /// </para>
    /// </summary>
    /// <remarks>
    /// XML-Doc comment/description taken straight from '<seealso cref="string.Substring(int, int)"/>'
    /// </remarks>
    /// <returns>
    ///     A string that is equivalent to the substring of <paramref name="maxLength"/> length that begins at startIndex in this instance, or
    ///     <c>string.Empty</c> if <paramref name="startIndex"/> is equal to the length of this instance and length is zero.
    /// </returns>
    public static string? Subsmarter(this string? str, int startIndex, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) {
            return str;
        }

        if (startIndex < 0) {
            startIndex = 0;
        }
        if (startIndex + maxLength >= str.Length) {
            return str.Substring(startIndex);
        }

        return str.Substring(startIndex, maxLength);
    }
}
