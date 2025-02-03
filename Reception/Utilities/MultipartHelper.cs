using System;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace Reception.Utilities;

/// <summary>
/// Lifted from example-implementation for File Streaming at Microsoft Learn.<br/>
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0#upload-large-files-with-streaming"/>
/// </summary>
public static class MultipartHelper
{
    public const uint FILE_SIZE_LIMIT = 8388608 * 16; // 32MB

    // Size thresholds.
    public const uint LARGE_FILE_THRESHOLD = 8388608 * 8; // 8MB
    public const string LARGE_FILE_CATEGORY_SLUG = "HD";
    public const uint SMALL_FILE_THRESHOLD = 8192 * 128; // 128KB
    public const string SMALL_FILE_CATEGORY_SLUG = "SD";

    // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
    // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException(
                $"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }

    public static bool IsMultipartContentType(string? contentType) => (
        !string.IsNullOrEmpty(contentType) && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0
    );

    public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition) => (
        // Content-Disposition: form-data; name="key";
        contentDisposition != null &&
        contentDisposition.DispositionType.Equals("form-data") &&
        string.IsNullOrEmpty(contentDisposition.FileName.Value) &&
        string.IsNullOrEmpty(contentDisposition.FileNameStar.Value)
    );

    public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition) => (
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        contentDisposition != null &&
        contentDisposition.DispositionType.Equals("form-data") && (
            !string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
            !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value)
        )
    );
}
