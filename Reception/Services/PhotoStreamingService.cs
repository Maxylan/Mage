using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Reception.Authentication;
using Reception.Models;
using Reception.Models.Entities;
using Reception.Utilities;
using Reception.Interfaces;
using System.Globalization;
using System.Text;
using System.Net;
using Reception.Constants;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Reception.Services;

public class PhotoStreamingService(
    IHttpContextAccessor contextAccessor,
    ILoggingService<PhotoStreamingService> logging,
    IPhotoService photoService,
    IIntelligenceService ai,
    MageDbContext db
) : IPhotoStreamingService
{
    #region Create / Store photos.
    /// <summary>
    /// Upload any amount of new photos/files (<see cref="PhotoEntity"/>, <seealso cref="Reception.Models.Entities.PhotoCollection"/>)
    /// by streaming them directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as options/details of the
    /// generated database entitities.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhotos(Action<PhotosOptions> opts)
    {
        FilterPhotosOptions options = new();
        opts(options);

        return UploadPhotos(options);
    }

    /// <summary>
    /// Upload any amount of new photos/files (<see cref="PhotoEntity"/>, <seealso cref="Reception.Models.Entities.PhotoCollection"/>)
    /// by streaming them directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="options"/>) has been repurposed to serve as options/details of the
    /// generated database entitities.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public async Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhotos(PhotosOptions options)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(UploadPhotos)} Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(UploadPhotos))
                .InternalError(message)
                .LogAndEnqueue();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (!MultipartHelper.IsMultipartContentType(httpContext.Request.ContentType))
        {
            string message = $"{nameof(UploadPhotos)} Failed: Request couldn't be processed, not a Multipart Formdata request.";
            logging
                .Action(nameof(UploadPhotos))
                .ExternalError(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        Account? user = null;

        if (MageAuthentication.IsAuthenticated(contextAccessor))
        {
            try
            {
                user = MageAuthentication.GetAccount(contextAccessor);
            }
            catch (Exception ex)
            {
                logging
                    .Action(nameof(UploadPhotos))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .LogAndEnqueue();
            }
        }

        if (user is not null)
        {
            options.UploadedBy = user.Id;
        }

        var mediaTypeHeader = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType!);
        string boundary = MultipartHelper.GetBoundary(mediaTypeHeader, 70);

        var reader = new MultipartReader(boundary, httpContext.Request.Body);
        MultipartSection? section;

        List<PhotoCollection> photos = [];
        Dictionary<int, Task<ActionResult<OllamaAnalysis>>> analysis = [];
        uint iteration = 0u;
        do
        {
            db.CancellationToken.ThrowIfCancellationRequested();

            section = await reader.ReadNextSectionAsync();
            if (section is null)
            {
                break;
            }

            bool hasContentDisposition =
                ContentDispositionHeaderValue.TryParse(section?.ContentDisposition, out var contentDisposition);

            if (!hasContentDisposition || contentDisposition is null)
            {
                continue;
            }

            if (MultipartHelper.HasFileContentDisposition(contentDisposition))
            {
                PhotoEntity? newPhoto;
                try
                {
                    newPhoto = await UploadSinglePhoto(
                        options,
                        contentDisposition,
                        section!,
                        user
                    );

                    if (newPhoto is null)
                    {
                        logging
                            .Action(nameof(UploadPhotos))
                            .InternalError($"Failed to create a {nameof(PhotoEntity)} using uploaded photo. {nameof(newPhoto)} was null.")
                            .LogAndEnqueue();
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    logging
                         .Action(nameof(UploadPhotos))
                           .InternalError($"Failed to upload a photo. ({ex.GetType().Name}) " + ex.Message, opts =>
                           {
                               opts.Exception = ex;
                           })
                         .LogAndEnqueue();
                    continue;
                }

                if (newPhoto.Id == default)
                {
                    // New photo needs to be uploaded to the database..
                    var createEntity = await photoService.CreatePhotoEntity(newPhoto);
                    newPhoto = createEntity.Value;

                    if (newPhoto is null || newPhoto.Id == default)
                    {
                        logging
                            .Action(nameof(UploadPhotos))
                            .InternalError($"Failed to create a {nameof(PhotoEntity)} using uploaded photo '{(newPhoto?.Slug ?? "null")}'. Entity was null, or its Photo ID remained as 'default' post-saving to the database ({(newPhoto?.Id.ToString() ?? "null")}).")
                            .LogAndEnqueue();
                        continue;
                    }
                }

                if (!newPhoto!.Filepaths.Any(path => path.IsSource))
                {
                    logging
                        .Action(nameof(UploadPhotos))
                        .InternalError($"No '{Dimension.SOURCE.ToString()}' {nameof(Filepath)} found in the newly uploaded/created {nameof(PhotoEntity)} instance '{newPhoto.Slug}' (#{newPhoto.Id}).")
                        .LogAndEnqueue();
                    continue;
                }

                db.CancellationToken.ThrowIfCancellationRequested();

                // Fire a query to the AI in-parallell to the rest of our operations.
                // These take a while, so it would be awesome if we could "fire-and-forget"-them in a non-blocking way.
                if (newPhoto.Filepaths.Any(p => p.Dimension == Dimension.THUMBNAIL))
                {
                    analysis.Add(
                        newPhoto.Id, ai.InferThumbnailImage(newPhoto, db.CancellationToken)
                    );
                    /* .ContinueWith(
                        imageAnalysis => this.ApplyPhotoAnalysis(imageAnalysis, newPhoto, db.CancellationToken)
                    ) */
                }
                else if (newPhoto.Filepaths.Any(p => p.Dimension == Dimension.MEDIUM))
                {
                    analysis.Add(
                        newPhoto.Id, ai.InferMediumImage(newPhoto, db.CancellationToken)
                    );
                    /* .ContinueWith(
                        imageAnalysis => this.ApplyPhotoAnalysis(imageAnalysis, newPhoto, db.CancellationToken)
                    ) */
                }

                photos.Add(
                    new(newPhoto)
                );
            }
            else if (MultipartHelper.HasFormDataContentDisposition(contentDisposition))
            {
                // TODO! Alternative way to get created-date?
                // if (string.IsNullOrWhiteSpace(contentDisposition.CreationDate))

                string sectionName = contentDisposition.Name.ToString().ToLower();
                if (sectionName == "title")
                {
                    string extractedTitle = await section!.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(extractedTitle))
                    {
                        options.Title = extractedTitle;
                    }

                    continue;
                }

                if (sectionName == "slug")
                {
                    string extractedSlug = await section!.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(extractedSlug))
                    {
                        options.Slug = extractedSlug;
                    }

                    continue;
                }

                if (sectionName == "tags")
                {
                    string tagsString = await section!.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(tagsString))
                    {
                        tagsString = tagsString.Replace(", ", ","); // Additional level of fault-tolerance..
                        options.Tags = tagsString
                            .Trim()
                            .Split(",")
                            .Where(_t => !string.IsNullOrWhiteSpace(_t))
                            .ToArray();
                    }

                    continue;
                }
            }
        }
        while (section is not null && ++iteration < 4096u);

        if (analysis.Any()) {
            logging
                .Action(nameof(UploadPhotos))
                .InternalInformation($"Performing '{analysis.Count()}' photo-analysis");

            List<Task>[] updatePhotos = [];
            foreach (var singlePhotoAnalysis in analysis) {
                var analysisResult = await singlePhotoAnalysis.Value;
                await ai.ApplyPhotoAnalysis(
                    singlePhotoAnalysis.Key,
                    analysisResult,
                    db.CancellationToken
                );
            }

            Console.WriteLine($"Done! Performed '{analysis.Count()}' analysis of uploaded photo(s)");
            /* await logging
                .Action(nameof(UploadPhotos))
                .InternalInformation($"Done! Performed '{analysis.Count()}' analysis of uploaded photo(s)")
                .LogAndEnqueue(); */
        }

        return new CreatedAtActionResult(null, null, null, photos);
    }

    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="PhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoEntity"/></returns>
    protected async Task<PhotoEntity> UploadSinglePhoto(
        PhotosOptions options,
        ContentDispositionHeaderValue contentDisposition,
        MultipartSection section,
        Account? user = null
    )
    {
        string sourcePath = string.Empty;
        string mediumPath = string.Empty;
        string thumbnailPath = string.Empty;
        string fileExtension = string.Empty;
        string trustedFilename = string.Empty;
        DateTime createdAt = DateTime.UtcNow; // Fallback in case no EXIF data..
        DateTime uploadedAt = DateTime.UtcNow;

        // Don't trust the file name sent by the client. To display the file name, HTML-encode the value.
        // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0#upload-large-files-with-streaming
        string? untrustedFilename = contentDisposition.FileName.Value;

        if (string.IsNullOrWhiteSpace(untrustedFilename))
        {
            throw new NotImplementedException("Handle case of no filename"); // TODO: HANDLE
        }

        if (!untrustedFilename.IsNormalized())
        {
            untrustedFilename = untrustedFilename
                .Normalize()
                .Trim();
        }

        trustedFilename = WebUtility.HtmlEncode(untrustedFilename);
        var filenameParts = trustedFilename.Split('.');
        string sanitizedName = string.Join("_", filenameParts[..^1])
            .Trim()
            .Replace(Path.DirectorySeparatorChar.ToString(), "\\" + Path.DirectorySeparatorChar)
            .Subsmart(0, 123);

        // Should be normalized, encoded, trimmed and clamped! The four avengers of trust
        trustedFilename = sanitizedName + "." + filenameParts[^1];

        if (trustedFilename.Contains("&&")
            || trustedFilename.Contains("..")
            || trustedFilename.Contains(Path.DirectorySeparatorChar)
            || trustedFilename.Length > 127
        )
        {
            throw new NotImplementedException("Suspicious upload? " + trustedFilename); // TODO! Handle!!
        }

        fileExtension = Path.GetExtension(trustedFilename).ToLowerInvariant();
        if (fileExtension.StartsWith('.') && fileExtension.Length > 1)
        {
            fileExtension = fileExtension[1..];
        }

        if (string.IsNullOrWhiteSpace(fileExtension) || !MimeVerifyer.SupportedExtensions.Contains(fileExtension))
        {
            throw new NotImplementedException("The extension is invalid.. discontinuing processing of the file"); // TODO: HANDLE
        }

        using MemoryStream requestStream = new MemoryStream();
        await section.Body.CopyToAsync(requestStream);

        if (requestStream.Length <= 0)
        {
            throw new NotImplementedException("The file is empty."); // TODO: HANDLE
        }
        if (requestStream.Length > MultipartHelper.FILE_SIZE_LIMIT)
        {
            throw new NotImplementedException("The file is too large ... discontinuing processing of the file"); // TODO: HANDLE
        }

        // Attempt to manually detect and return image format. Also calls `MimeVerifyer.ValidateContentType()`
        IImageFormat? format = MimeVerifyer.DetectImageFormat(trustedFilename, fileExtension, requestStream);

        if (format is null)
        {   // TODO! Handle below scenarios!
            // - Bad filename/extension string
            // - Extension/Filename missmatch
            // - MIME not supported
            // - MIME not a valid image type.
            // - MIME not supported by ImageSharp / Missing ImageFormat
            // - MIME Could not be validated (bad magic numbers)
            throw new NotImplementedException($"{nameof(UploadSinglePhoto)} {nameof(format)} is null."); // TODO! Handle!!
        }

        sourcePath = photoService.GetCombinedPath(Dimension.SOURCE, uploadedAt);
        mediumPath = photoService.GetCombinedPath(Dimension.MEDIUM, uploadedAt);
        thumbnailPath = photoService.GetCombinedPath(Dimension.THUMBNAIL, uploadedAt);
        // TODO! Parse EXIF before combining paths to use the date the picture was taken/created as its path, instead of the upload date.

        try
        {
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(mediumPath);
            Directory.CreateDirectory(thumbnailPath);
        }
        catch (Exception ex)
        {
            /* // Handle billions of different exceptions, maybe..
	        IOException
	        UnauthorizedAccessException
	        ArgumentException
	        ArgumentNullException
	        PathTooLongException
	        DirectoryNotFoundException
	        NotSupportedException
	        */
            throw new NotImplementedException($"Handle directory create errors {nameof(UploadSinglePhoto)} ({sourcePath}) " + ex.Message); // TODO: HANDLE
        }

        // Handle potential name-conflicts on the path..
        string filename = trustedFilename;
        string fullPath = Path.Combine(sourcePath, filename);
        int extensionIndex = trustedFilename.LastIndexOf('.');
        int conflicts = 0;

        while (File.Exists(fullPath) && ++conflicts <= 4096)
        {
            string appendix = "_copy";
            if (conflicts > 1)
            {
                appendix += "_" + (conflicts - 1);
            }

            filename = extensionIndex != -1
                ? trustedFilename.Insert(extensionIndex, appendix)
                 : (trustedFilename + appendix);

            fullPath = Path.Combine(sourcePath, filename);
        }

        // Saved! :)
        using FileStream file = File.Create(fullPath);
        await file.WriteAsync(requestStream.ToArray());
        await requestStream.DisposeAsync();
        file.Position = 0;

        long filesize = file.Length;
        long mediumFilesize = 0L;
        long thumbnailFilesize = 0L;
        Size sourceDimensions;
        Size mediumDimensions;
        Size thumbnailDimensions;
        string dpi = string.Empty;
        string sourceFilesizeFormatted = filesize.ToString();

        if (filesize < 8388608)
        { // Kilobytes..
            sourceFilesizeFormatted = $"{Math.Round((decimal)(filesize / 8192), 1)}kB";
        }
        else if (filesize >= 8589934592)
        { // Gigabytes..
            sourceFilesizeFormatted = $"{Math.Round((decimal)(filesize / 8589934592), 3)}GB";
        }
        else
        { // Megabytes..
            sourceFilesizeFormatted = $"{Math.Round((decimal)(filesize / 8388608), 2)}MB";
        }

        Configuration imageConfiguration = Configuration.Default.Clone();
        imageConfiguration.ReadOrigin = 0;

        DecoderOptions decoderOptions = new()
        {
            Configuration = imageConfiguration
        };

        // Acquire decoder/encoder(s) for the identified format..
        IImageDecoder imageDecoder = decoderOptions.Configuration.ImageFormatsManager.GetDecoder(format);
        if (imageDecoder is null)
        {
            throw new NotImplementedException($"{nameof(UploadSinglePhoto)} {nameof(imageDecoder)} is null."); // TODO! Handle!!
        }

        IImageEncoder imageEncoder = decoderOptions.Configuration.ImageFormatsManager.GetEncoder(format);
        if (imageEncoder is null)
        {
            throw new NotImplementedException($"{nameof(UploadSinglePhoto)} {nameof(imageEncoder)} is null."); // TODO! Handle!!
        }

        using (Image source = await imageDecoder.DecodeAsync(decoderOptions, file))
        {
            string alternatePath = fullPath;
            sourceDimensions = source.Size;

            var exifProfile = source.Metadata.ExifProfile;
            if (exifProfile is not null && exifProfile.TryGetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeOriginal, out var pictureTakenAt))
            {
                if (DateTime.TryParse(
                    pictureTakenAt.Value,
                    System.Globalization.CultureInfo.CurrentCulture,
                    DateTimeStyles.AdjustToUniversal,
                    out createdAt
                ))
                {
                    if (Program.IsDevelopment)
                    {
                        Console.WriteLine($"(Debug -> TryParse) Uploading image '{trustedFilename}' from (EXIF) '{pictureTakenAt.Value}' parsed as '{createdAt}'.");
                    }
                }
                else if (DateTime.TryParseExact(
                    pictureTakenAt.Value,
                    "yyyy:MM:dd HH:mm:ss", // Weird, example: 2024:07:20 15:12:48
                    System.Globalization.CultureInfo.CurrentCulture,
                    DateTimeStyles.AdjustToUniversal,
                    out createdAt
                ))
                {
                    if (Program.IsDevelopment)
                    {
                        Console.WriteLine($"(Debug -> TryParseExact) Uploading image '{trustedFilename}' from (EXIF) '{pictureTakenAt.Value}' parsed as '{createdAt}'.");
                    }
                }
            }

            double dpiX = Math.Round(source.Metadata.HorizontalResolution, 1);
            double dpiY = Math.Round(source.Metadata.VerticalResolution, 1);

            if (dpiX == default)
            {
                dpi = dpiY.ToString();
            }
            else if (dpiY == default)
            {
                dpi = dpiX.ToString();
            }
            else if (dpiX == dpiY)
            {
                dpi = dpiX.ToString();
            }
            else
            {
                dpi = $"{dpiX}x{dpiY}";
            }

            decimal aspectRatio = Math.Max(sourceDimensions.Width, sourceDimensions.Height) / Math.Min(sourceDimensions.Width, sourceDimensions.Height);

            mediumDimensions = new(
                (int)(Math.Clamp(ImageDimensions.Medium.TARGET * aspectRatio, ImageDimensions.Medium.CLAMP_MINIMUM, ImageDimensions.Medium.CLAMP_MAXIMUM)),
                (int)(Math.Clamp(ImageDimensions.Medium.TARGET * aspectRatio, ImageDimensions.Medium.CLAMP_MINIMUM, ImageDimensions.Medium.CLAMP_MAXIMUM))
            );

            if (sourceDimensions.Width > mediumDimensions.Width && sourceDimensions.Height > mediumDimensions.Height)
            {
                using Image medium = source.Clone(context =>
                {
                    context.Resize(new ResizeOptions()
                    {
                        Size = mediumDimensions,
                        Mode = ResizeMode.Max
                    });
                });

                mediumDimensions.Width = medium.Width;
                mediumDimensions.Height = medium.Height;
                alternatePath = Path.Combine(mediumPath, filename);

                using (var mediumFile = File.Create(alternatePath))
                {
                    await medium.SaveAsync(mediumFile, imageEncoder);
                    mediumFilesize = mediumFile.Length;
                    Console.WriteLine($"mediumFilesize {mediumFilesize}");
                }
            }

            thumbnailDimensions = new(
                (int)(Math.Clamp(ImageDimensions.Thumbnail.TARGET * aspectRatio, ImageDimensions.Thumbnail.CLAMP_MINIMUM, ImageDimensions.Thumbnail.CLAMP_MAXIMUM)),
                (int)(Math.Clamp(ImageDimensions.Thumbnail.TARGET * aspectRatio, ImageDimensions.Thumbnail.CLAMP_MINIMUM, ImageDimensions.Thumbnail.CLAMP_MAXIMUM))
            );

            if (sourceDimensions.Width > thumbnailDimensions.Width && sourceDimensions.Height > thumbnailDimensions.Height)
            {
                using Image thumbnail = source.Clone(context =>
                {
                    context.Resize(new ResizeOptions()
                    {
                        Size = thumbnailDimensions,
                        Mode = ResizeMode.Max
                    });
                });

                thumbnailDimensions.Width = thumbnail.Width;
                thumbnailDimensions.Height = thumbnail.Height;
                alternatePath = Path.Combine(thumbnailPath, filename);

                using (var thumbnailFile = File.Create(alternatePath))
                {
                    await thumbnail.SaveAsync(thumbnailFile, imageEncoder);
                    thumbnailFilesize = thumbnailFile.Length;
                    Console.WriteLine($"thumbnailFilesize {thumbnailFilesize}");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(options.Slug))
        {
            // `filename` is only guaranteed unique *for a single date (24h)*.
            options.Slug = $"{uploadedAt.ToShortDateString().Replace('/', '-')}-{filename}";

            extensionIndex = options.Slug.LastIndexOf('.');
            if (extensionIndex != -1)
            {
                options.Slug = options.Slug[..extensionIndex];
            }

            if (options.Slug.Length > 123)
            { // Limit length, to avoid hitting the limit of 128 with the auto-generated slug.
                options.Slug = options.Slug.Subsmart(0, 120) + "_" + options.Slug.Length;
            }

            // Resolve possible (..yet, unlikely) ..conflicts/duplicate slugs:
            int count = await db.Photos.CountAsync(photo => photo.Slug == options.Slug);
            if (count > 0)
            {
                options.Slug += "_" + count;
            }
        }
        else if (!options.Slug.IsNormalized())
        {
            options.Slug = options.Slug
                .Normalize()
                .Trim();
        }

        if (options.Slug.Length > 127)
        {
            string message = $"Failed to upload photo, {nameof(PhotoEntity.Slug)} exceeds maximum allowed length of 127.";
            logging
                .Action(nameof(UploadSinglePhoto))
                .InternalWarning(message)
                .LogAndEnqueue();

            throw new Exception(message); // TODO! Handle more gracefully? Maybe clean up saved image from disk as well.
        }

        if (string.IsNullOrWhiteSpace(options.Title))
        {
            options.Title = trustedFilename;
        }
        else if (!options.Title.IsNormalized())
        {
            options.Title = options.Title
                .Normalize()
                .Trim();
        }
        if (conflicts > 0)
        {
            options.Title += $" (#{conflicts})";
        }

        if (options.Title.Length > 255)
        {
            string message = $"Failed to upload photo, {nameof(PhotoEntity.Title)} exceeds maximum allowed length of 255.";
            logging
                .Action(nameof(UploadSinglePhoto))
                .InternalWarning(message)
                .LogAndEnqueue();

            throw new Exception(message); // TODO! Handle more gracefully? Maybe clean up saved image from disk as well.
        }

        if (string.IsNullOrWhiteSpace(options.Summary))
        {
            options.Summary = $"{options.Title} - ";
            if (sourceDimensions.Width != default && sourceDimensions.Height != default)
            {
                options.Summary += $"{sourceDimensions.Width}x{sourceDimensions.Height}, {sourceFilesizeFormatted}.";
            }
            else
            {
                options.Summary += $"{sourceFilesizeFormatted}.";
            }
        }
        else if (!options.Summary.IsNormalized())
        {
            options.Summary = options.Summary
                .Normalize()
                .Trim();
        }

        if (options.Summary.Length > 255)
        {   // Think this is the better option for summaries, since they're optional..
            options.Summary = options.Summary.Subsmart(0, 253) + "..";
        }

        StringBuilder formattedDescription = new();
        DateTime createdOrUploadedDate = createdAt != uploadedAt ? createdAt : uploadedAt;
        string createdOrUploadedText = createdAt != uploadedAt
            ? "Taken/Created "
            : "Uploaded ";

        formattedDescription.Append(createdOrUploadedText);

        formattedDescription.Append(createdOrUploadedDate.Month switch
        {
            1 => "January",
            2 => "February",
            3 => "March",
            4 => "April",
            5 => "May",
            6 => "June",
            7 => "July",
            8 => "August",
            9 => "September",
            10 => "October",
            11 => "November",
            12 => "December",
            _ => "Unknown"
        });

        formattedDescription.AppendFormat(" {0}", createdOrUploadedDate.Year);

        formattedDescription.AppendFormat(", saved to '{0}' (", sourcePath);

        if (sourceDimensions.Width != default && sourceDimensions.Height != default)
        {
            formattedDescription.AppendFormat("{0}x", sourceDimensions.Width);
            formattedDescription.AppendFormat("{0}, ", sourceDimensions.Height);
        }

        formattedDescription.AppendFormat("{0})", sourceFilesizeFormatted);

        if (conflicts > 0)
        {
            formattedDescription.AppendFormat(". Potentially a copy of {0} other files.", conflicts);
        }

        // TODO! Replace DB Call here with some tags service..

        List<Tag> tags = [];
        if (options.Tags is not null && options.Tags.Length > 0)
        {
            foreach (string tagName in options.Tags.Distinct())
            {
                Tag? tag = await db.Tags.FirstOrDefaultAsync(_t => _t.Name == tagName);

                if (tag is null)
                {
                    tag = new()
                    {
                        Name = tagName
                    };
                }

                tags.Add(tag);
            }
        }

        PhotoEntity photo = new()
        {
            Slug = options.Slug,
            Title = options.Title,
            Summary = options.Summary,
            Description = formattedDescription.ToString(),
            UploadedBy = options.UploadedBy,
            UploadedAt = uploadedAt,
            CreatedAt = createdAt,
            UpdatedAt = DateTime.UtcNow,
            Tags = tags,
            Filepaths = [
                new() {
                    Filename = filename,
                    Path = sourcePath,
                    Dimension = Dimension.SOURCE,
                    Filesize = filesize,
                    Height = sourceDimensions.Height,
                    Width = sourceDimensions.Width
                }
            ]
        };

        if (user is not null)
        {
            photo.UploadedBy = user.Id;
        }

        // Auto-generated Filepaths..
        if (mediumFilesize > 0)
        {
            photo.Filepaths.Add(new()
            {
                Filename = filename,
                Path = mediumPath,
                Dimension = Dimension.MEDIUM,
                Filesize = mediumFilesize,
                Height = mediumDimensions.Height,
                Width = mediumDimensions.Width
            });
        }

        if (thumbnailFilesize > 0)
        {
            photo.Filepaths.Add(new()
            {
                Filename = filename,
                Path = thumbnailPath,
                Dimension = Dimension.THUMBNAIL,
                Filesize = thumbnailFilesize,
                Height = thumbnailDimensions.Height,
                Width = thumbnailDimensions.Width
            });
        }

        // Auto-generated Tags..
        if (createdAt != uploadedAt)
        {
            // TODO! Replace DB Call here with some tags service..
            var year_tag = await db.Tags.FirstOrDefaultAsync(
                tag => tag.Name == createdAt.Year.ToString(System.Globalization.CultureInfo.CurrentCulture)
            );

            photo.Tags.Add(year_tag ?? new()
            {
                Name = createdAt.Year.ToString(System.Globalization.CultureInfo.CurrentCulture),
                Description = "Images taken/created during " + createdAt.Year.ToString(System.Globalization.CultureInfo.CurrentCulture)
            });
        }
        if (filesize >= MultipartHelper.LARGE_FILE_THRESHOLD)
        {
            // TODO! Replace DB Call here with some tags service..
            var hd_tag = await db.Tags.FirstOrDefaultAsync(
                tag => tag.Name == MultipartHelper.LARGE_FILE_CATEGORY_SLUG
            );

            photo.Tags.Add(hd_tag ?? new()
            {
                Name = MultipartHelper.LARGE_FILE_CATEGORY_SLUG,
                Description = "Large or High-Definition Images."
            });
        }
        else if (filesize < MultipartHelper.SMALL_FILE_THRESHOLD)
        {
            // TODO! Replace DB Call here with some tags service..
            var sd_tag = await db.Tags.FirstOrDefaultAsync(
                tag => tag.Name == MultipartHelper.SMALL_FILE_CATEGORY_SLUG
            );

            photo.Tags.Add(sd_tag ?? new()
            {
                Name = MultipartHelper.SMALL_FILE_CATEGORY_SLUG,
                Description = "Small or Low-Definition Images and/or thumbnails."
            });
        }
        if (conflicts > 0)
        {
            // TODO! Replace DB Call here with some tags service..
            var copy_tag = await db.Tags.FirstOrDefaultAsync(
                tag => tag.Name == "Copy"
            );

            photo.Tags.Add(copy_tag ?? new()
            {
                Name = "Copy",
                Description = "Image might be a copy of another, its filename conflicts with at least one other file uploaded around the same time."
            });
        }

        logging
            .Action(nameof(UploadSinglePhoto))
            .ExternalInformation($"Finished streaming file '{filename}' to '{sourcePath}' and generated {nameof(PhotoCollection)} '{photo.Slug}'.")
            .LogAndEnqueue();

        // Reset provided 'options' to defaults.
        // Order matters, since we're uploading more than one file..
        options.Slug = null;
        options.Title = null;
        options.Tags = null;

        return photo;
    }
    #endregion
}
