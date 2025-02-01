using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace Reception.Utilities;

/// <summary>
/// Inspired by examples found at Microsoft Learn.<br/>
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0#upload-large-files-with-streaming"/>
/// </summary>
public static class MimeVerifyer
{
    /// <summary>
    /// "Magic Numbers" of various MIME Types / Content Types.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     <see href="https://en.wikipedia.org/wiki/List_of_file_signatures"/>
    /// </para>
    /// <para>
    ///     <see href="https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0#upload-large-files-with-streaming"/>
    /// </para>
    /// </remarks>
    public static readonly Dictionary<string, (uint, IReadOnlyCollection<byte[]>)> MagicNumbers =
        new Dictionary<string, (uint, IReadOnlyCollection<byte[]>)>{
            { "jpeg", (
                0u, [
                    [0xFF, 0xD8, 0xFF, 0xDB],
                    [0xFF, 0xD8, 0xFF, 0xEE],
                    [0xFF, 0xD8, 0xFF, 0xE1],
                    [0xFF, 0xD8, 0xFF, 0xE2],
                    [0xFF, 0xD8, 0xFF, 0xE3]
                ]
            )},
            { "jpg", (
                0u, [
                    [0xFF, 0xD8, 0xFF, 0xDB],
                    [0xFF, 0xD8, 0xFF, 0xE0],
                    [0xFF, 0xD8, 0xFF, 0xE1],
                    [0xFF, 0xD8, 0xFF, 0xE2],
                    [0xFF, 0xD8, 0xFF, 0xE3],
                    [0xFF, 0xD8, 0xFF, 0xEE]
                ]
            )},
            { "jp2", (  // JPEG 2000
                0u, [
                    [0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A]
                ]
            )},
            { "jpg2", ( // JPEG 2000
                0u, [
                    [0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A]
                ]
            )},
            { "jpm", (  // JPEG 2000
                0u, [
                    [0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A]
                ]
            )},
            { "jpc", (  // JPEG 2000
                0u, [
                    [0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A],
                    [0xFF, 0x4F, 0xFF, 0x51]
                ]
            )},
            { "png", (
                0u, [
                    [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]
                ]
            )},
            { "pdf", (
                0u, [
                    [0x25, 0x50, 0x44, 0x46, 0x2D]
                ]
            )},
            { "ico", (
                0u, [
                    [0x00, 0x00, 0x01, 0x00]
                ]
            )},
            { "icns", (
                0u, [
                    [0x69, 0x63, 0x6E, 0x73]
                ]
            )},
            { "heic", (
                0u, [
                    [0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63],
                    [0x66, 0x74, 0x79, 0x70, 0x6d]
                ]
            )},
            { "tif", (
                0u, [
                    [0x49, 0x49, 0x2A, 0x00], // BE
                    [0x4D, 0x4D, 0x00, 0x2A], // LE
                    [0x49, 0x49, 0x2B, 0x00], // BE
                    [0x4D, 0x4D, 0x00, 0x2B] // LE
                ]
            )},
            { "tiff", (
                0u, [
                    [0x49, 0x49, 0x2A, 0x00], // BE
                    [0x4D, 0x4D, 0x00, 0x2A], // LE
                    [0x49, 0x49, 0x2B, 0x00], // BE
                    [0x4D, 0x4D, 0x00, 0x2B] // LE
                ]
            )},
            { "gif", (
                0u, [
                    [0x47, 0x49, 0x46, 0x38, 0x37, 0x61],
                    [0x47, 0x49, 0x46, 0x38, 0x39, 0x61]
                ]
            )},
            { "ogg", (
                0u, [
                    [0x4F, 0x67, 0x67, 0x53]
                ]
            )},
            { "oga", (
                0u, [
                    [0x4F, 0x67, 0x67, 0x53]
                ]
            )},
            { "ogv", (
                0u, [
                    [0x4F, 0x67, 0x67, 0x53]
                ]
            )},
            { "mp3", (
                0u, [
                    [0xFF, 0xFB],
                    [0xFF, 0xF3],
                    [0xFF, 0xF2],
                    [0x49, 0x44, 0x33] // ID3
                ]
            )},
            { "mp3_id3", (
                0u, [
                    [0x49, 0x44, 0x33]
                ]
            )},
            { "webp", ( // Special case.. requires a file size.
                0u, [   // https://developers.google.com/speed/webp/docs/riff_container#webp_file_header
                    [0x52, 0x49, 0x46, 0x46],
                    [0x57, 0x45, 0x42, 0x50]
                ]
            )},
            { "mpg", (
                0u, [
                    [0x00, 0x00, 0x01, 0xB3]
                ]
            )},
            { "mpeg", (
                0u, [
                    [0x00, 0x00, 0x01, 0xB3]
                ]
            )},
            { "mp4", (
                4u, [
                    [0x66, 0x74, 0x79, 0x70, 0x4D, 0x53, 0x4E, 0x56],
                    [0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D]
                ]
            )},
            { "mp4_iso", (
                4u, [
                    [0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D]
                ]
            )},
            { "flv", (
                0u, [
                    [0x46, 0x4C, 0x56]
                ]
            )},
            { "hdr", (
                0u, [
                    [0x23, 0x3F, 0x52, 0x41, 0x44, 0x49, 0x41, 0x4E, 0x43, 0x45, 0x0A]
                ]
            )}
        };

    /// <summary>
    /// MIME Types / Extensions supported for upload.
    /// </summary>
    public static readonly IReadOnlyCollection<string> SupportedExtensions = MimeVerifyer.MagicNumbers.Keys;


    /// <summary>
    /// Check the first few bytes of the stream, i.e the "Magic Numbers", to validate that
    /// the contentType of the stream matches what was given as its '<paramref name="extension"/>'
    /// </summary>
    public static bool ValidateContentType(string filename, string extension, Stream stream)
    {
        if (!filename.EndsWith("."+extension)) {
            throw new NotImplementedException("Filename does not end with the given extension!"); // TODO: HANDLE
        }
        if (!SupportedExtensions.Contains(extension)) {
            throw new NotImplementedException("File extension not supported!"); // TODO: HANDLE
        }

        using BinaryReader reader = new BinaryReader(stream);

        int offset = (int)MagicNumbers[extension].Item1;
        int longestSignature = MagicNumbers[extension].Item2.Max(m => m.Length);

        // https://stackoverflow.com/questions/4883618/empty-array-with-binaryreader-on-uploadedfile-in-c-sharp
        // Debug: Console.WriteLine($"Skipping first {offset} bytes..");
        reader.BaseStream.Position = offset;
        var headerBytes = reader.ReadBytes(longestSignature);

        return MagicNumbers[extension].Item2 // 'if any signatures in MagicNumbers[..] matches `headerBytes`'..
            .Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}
