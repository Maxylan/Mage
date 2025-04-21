using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp.Formats;
using System.Globalization;

namespace Reception.Utilities;

/// <summary>
/// My own implimentation of an ImageFormatDetector & MimeVerifyer, inspired by examples found at Microsoft Learn.<br/>
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0#upload-large-files-with-streaming"/>
/// </summary>
public class MimeVerifyer : IImageFormatDetector
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
                    [0xFF, 0xD8, 0xFF, 0xE0],
                    [0xFF, 0xD8, 0xFF, 0xE1],
                    [0xFF, 0xD8, 0xFF, 0xE2],
                    [0xFF, 0xD8, 0xFF, 0xE3],
                    [0xFF, 0xD8, 0xFF, 0xEE]
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
            { "bmp", (
                0u, [
                    [0x42, 0x4D]
                ]
            )},
            { "dib", (
                0u, [
                    [0x42, 0x4D]
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
    /// Convert a MIME Type (string) to a <see cref="IImageFormat"/> instance.
    /// </summary>
    public static IImageFormat? GetImageFormat(string key) => key switch
    {
        "jpeg" => SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance,
        "jpg" => SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance,
        "jp2" => null,
        "jpg2" => null,
        "jpm" => null,
        "jpc" => null,
        "png" => SixLabors.ImageSharp.Formats.Png.PngFormat.Instance,
        "bmp" => SixLabors.ImageSharp.Formats.Bmp.BmpFormat.Instance,
        "dib" => SixLabors.ImageSharp.Formats.Bmp.BmpFormat.Instance,
        "pdf" => null,
        "ico" => null,
        "icns" => null,
        "heic" => null,
        "tif" => SixLabors.ImageSharp.Formats.Tiff.TiffFormat.Instance,
        "tiff" => SixLabors.ImageSharp.Formats.Tiff.TiffFormat.Instance,
        "gif" => SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance,
        "ogg" => null,
        "oga" => null,
        "ogv" => null,
        "mp3" => null,
        "mp3_id3" => null,
        "webp" => SixLabors.ImageSharp.Formats.Webp.WebpFormat.Instance,
        "mpg" => null,
        "mpeg" => null,
        "mp4" => null,
        "mp4_iso" => null,
        "flv" => null,
        "hdr" => null,
        _ => throw new InvalidOperationException(
            $"{(string.IsNullOrWhiteSpace(key) ? "'null'" : key)} is completely unsupported by {nameof(MimeVerifyer)}"
        )
    };

    /// <summary>
    /// Check the first few bytes of the stream, i.e the "Magic Numbers", to validate that
    /// the contentType of the stream matches what was given as its '<paramref name="extension"/>'
    /// </summary>
    public static bool ValidateContentType(string filename, Stream stream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename, nameof(filename));

        string extension = filename;
        int lastDotIndex = filename.LastIndexOf(".");
        if (lastDotIndex != -1)
        {
            extension = filename[(lastDotIndex + 1)..];
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            ArgumentException argException = new(
                $"{nameof(MimeVerifyer.ValidateContentType)} was passed a *really* bad {nameof(filename)} ({filename}). Better look into that.",
                nameof(filename)
            );

            if (Program.IsDevelopment)
            {
                Console.WriteLine(argException.Message);
            }

            throw argException;
        }

        return ValidateContentType(filename, extension, stream);
    }
    /// <summary>
    /// Check the first few bytes of the stream, i.e the "Magic Numbers", to validate that
    /// the contentType of the stream matches what was given as its '<paramref name="extension"/>'
    /// </summary>
    public static bool ValidateContentType(string filename, string extension, Stream stream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension, nameof(extension));
        ArgumentException.ThrowIfNullOrWhiteSpace(filename, nameof(filename));

        int lastDotIndex = extension.LastIndexOf(".");
        if (lastDotIndex != -1)
        {
            string originalExtensionArgument = extension;
            extension = extension[(lastDotIndex + 1)..];

            if (string.IsNullOrWhiteSpace(extension))
            {
                ArgumentException argException = new(
                    $"{nameof(MimeVerifyer.ValidateContentType)} was passed a *really* bad {nameof(extension)} ({originalExtensionArgument}). Better look into that.",
                    nameof(extension)
                );

                if (Program.IsDevelopment)
                {
                    Console.WriteLine(argException.Message);
                }

                throw argException;
            }
        }

        if (!filename.EndsWith(extension, true, CultureInfo.InvariantCulture))
        {
            throw new NotImplementedException("Filename does not end with the given extension!"); // TODO: HANDLE
        }
        if (!SupportedExtensions.Contains(extension))
        {   // Ensure it isn't a poorly-formatted extension name preventing us from advancing..
            extension = extension
                .Normalize()
                .Trim()
                .ToLower();

            if (!SupportedExtensions.Contains(extension)) {
                throw new NotImplementedException($"File extension '{extension}' not supported!"); // TODO: HANDLE
            }
        }

        int offset = (int)MagicNumbers[extension].Item1;
        stream.Position = offset;

        BinaryReader reader = new BinaryReader(stream);

        // Special case.. expects the file's size as bytes 4..8.
        // https://developers.google.com/speed/webp/docs/riff_container#webp_file_header
        if (extension == "webp")
        {
            byte[] webpHeader = reader.ReadBytes(12);
            bool signatureOneMatch = webpHeader[0..4].SequenceEqual(MagicNumbers["webp"].Item2.ElementAt(0));
            bool signatureTwoMatch = webpHeader[8..12].SequenceEqual(MagicNumbers["webp"].Item2.ElementAt(1));

            return signatureOneMatch && signatureTwoMatch;
        }

        byte[] headerBytes = reader.ReadBytes(
            MagicNumbers[extension].Item2.Max(m => m.Length)
        );

        return MagicNumbers[extension].Item2 // 'if any signatures in MagicNumbers[..] matches `headerBytes`'..
            .Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
    }

    /// <summary>
    /// Check the first few bytes of the stream, i.e the "Magic Numbers", to validate that
    /// the contentType of the stream matches what was given as its '<paramref name="extension"/>'.
    /// <para>
    /// Returns a <see cref="IImageFormat"/> instance matching the parsed/detected MIME-Type.
    /// </para>
    /// </summary>
    public static IImageFormat? DetectImageFormat(string filename, Stream stream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename, nameof(filename));

        string extension = filename;
        int lastDotIndex = filename.LastIndexOf(".");
        if (lastDotIndex != -1)
        {
            extension = filename[(lastDotIndex + 1)..];
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            ArgumentException argException = new(
                $"{nameof(MimeVerifyer.DetectImageFormat)} was passed a *really* bad {nameof(filename)} ({filename}). Better look into that.",
                nameof(filename)
            );

            if (Program.IsDevelopment)
            {
                Console.WriteLine(argException.Message);
            }

            throw argException;
        }

        return DetectImageFormat(filename, extension, stream);
    }
    /// <summary>
    /// Check the first few bytes of the stream, i.e the "Magic Numbers", to validate that
    /// the contentType of the stream matches what was given as its '<paramref name="extension"/>'
    /// <para>
    /// Returns a <see cref="IImageFormat"/> instance matching the parsed/detected MIME-Type.
    /// </para>
    /// </summary>
    public static IImageFormat? DetectImageFormat(string filename, string extension, Stream stream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension, nameof(extension));
        ArgumentException.ThrowIfNullOrWhiteSpace(filename, nameof(filename));

        int lastDotIndex = extension.LastIndexOf(".");
        if (lastDotIndex != -1)
        {
            string originalExtensionArgument = extension;
            extension = extension[(lastDotIndex + 1)..];

            if (string.IsNullOrWhiteSpace(extension))
            {
                ArgumentException argException = new(
                    $"{nameof(MimeVerifyer.DetectImageFormat)} was passed a *really* bad {nameof(extension)} ({originalExtensionArgument}). Better look into that.",
                    nameof(extension)
                );

                if (Program.IsDevelopment)
                {
                    Console.WriteLine(argException.Message);
                }

                throw argException;
            }
        }

        bool isGivenExtensionValid = ValidateContentType(filename, extension, stream);
        if (isGivenExtensionValid)
        {
            return GetImageFormat(extension.ToLower());
        }
        else if (Program.IsDevelopment)
        {
            Console.WriteLine($"(Debug) ({nameof(DetectImageFormat)}) {nameof(ValidateContentType)} Deemed the extension '{extension}' on file '{filename}' invalid. Could be that the MIME is unspported, naming missmatches, or invalid Magic Numbers.");
        }

        return null;
    }

    // -- IImageFormatDetector

    public int HeaderSize { get; private set; }

    public bool TryDetectFormat(ReadOnlySpan<byte> header, [NotNullWhen(true)] out IImageFormat? format)
    {
        format = null;
        foreach (var mime in MagicNumbers)
        {
            // Special case.. expects the file's size as bytes 4..8.
            // https://developers.google.com/speed/webp/docs/riff_container#webp_file_header
            if (mime.Key == "webp")
            {
                if (header.Length < 12)
                {
                    continue;
                }

                bool signatureOneMatch = header[0..4].SequenceEqual(mime.Value.Item2.ElementAt(0));
                bool signatureTwoMatch = header[8..12].SequenceEqual(mime.Value.Item2.ElementAt(1));

                if (signatureOneMatch && signatureTwoMatch)
                {
                    HeaderSize = 12;
                    format = SixLabors.ImageSharp.Formats.Webp.WebpFormat.Instance;
                    return true;
                }

                continue;
            }

            foreach (var signature in mime.Value.Item2)
            {
                if (signature.Length > header.Length)
                {
                    continue;
                }

                if (header[(int)mime.Value.Item1..signature.Length].SequenceEqual(signature))
                {
                    HeaderSize = (int)mime.Value.Item1 + signature.Length;
                    format = GetImageFormat(mime.Key);

                    if (format is null && Program.IsDevelopment)
                    {
                        return false;
                    }

                    return format is not null;
                }
            }
        }

        return format is not null;
    }
}
