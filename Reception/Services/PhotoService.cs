
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Reception.Authentication;
using Reception.Models;
using Reception.Models.Entities;
using Reception.Utilities;
using Reception.Interfaces;
using Reception.Controllers;
using System.Globalization;
using System.Text;
using System.Net;

namespace Reception.Services;

public class PhotoService(
    MageDbContext db,
    ILoggingService logging,
    IHttpContextAccessor contextAccessor
) : IPhotoService
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
    public string GetBaseDirectoryName() => FILE_STORAGE_NAME;
    /// <summary>
    /// Get the name (only) of the Thumbnail directory of my file storage
    /// </summary>
    public string GetThumbnailDirectoryName() => StorageDirectories[Dimension.THUMBNAIL];
    /// <summary>
    /// Get the name (only) of the Medium directory of my file storage
    /// </summary>
    public string GetMediumDirectoryName() => StorageDirectories[Dimension.MEDIUM];
    /// <summary>
    /// Get the name (only) of the Source directory of my file storage
    /// </summary>
    public string GetSourceDirectoryName() => StorageDirectories[Dimension.SOURCE];
    /// <summary>
    /// Get the path (directories, plural) to the directory relative to a <see cref="DateTime"/>
    /// </summary>
    public string GetDatePath(DateTime dateTime) => Path.Combine(
        dateTime.Year.ToString(),
        dateTime.Month.ToString(),
        dateTime.Day.ToString()
    );
    /// <summary>
    /// Get the <strong>combined</strong> relative path (<c>Base + Thumbnail/Medium/Source + DatePath</c>) to a directory in my file storage.
    /// </summary>
    public string GetCombinedPath(Dimension dimension, DateTime? dateTime = null, string filename = "") => Path.Combine(
        GetBaseDirectoryName(),
        dimension switch {
            Dimension.THUMBNAIL => GetThumbnailDirectoryName(),
            Dimension.MEDIUM => GetMediumDirectoryName(),
            Dimension.SOURCE => GetSourceDirectoryName(),
            _ => GetSourceDirectoryName()
        },
        GetDatePath(dateTime ?? DateTime.UtcNow),
        filename
    );
    #endregion


    #region Get single photos.
    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> GetPhotoEntity(int photoId)
    {
        if (photoId <= 0) {
            throw new ArgumentException($"Parameter {nameof(photoId)} has to be a non-zero positive integer!", nameof(photoId));
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} matching the given {nameof(photoId)} #{photoId}.";
            await logging
                .Action(nameof(GetPhotoEntity))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (photo.Filepaths is null || photo.Filepaths.Count == 0)
        {
            // Load missing navigation entries.
            foreach(var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded) {
                    await navigation.LoadAsync();
                }
            }
        }

        return photo;
    }

    /// <summary>
    /// Get the <see cref="PhotoEntity"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> GetPhotoEntity(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        PhotoEntity? photo = await db.Photos
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Tags)
            .FirstOrDefaultAsync(photo => photo.Slug == slug);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} matching the given {nameof(slug)} '{slug}'.";
            await logging
                .Action(nameof(GetPhotoEntity))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return photo;
    }


    /// <summary>
    /// Get the <see cref="Reception.Models.Photo"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<Photo>> GetSinglePhoto(int photoId, Dimension dimension = Dimension.SOURCE)
    {
        var getEntity = await this.GetPhotoEntity(photoId);
        PhotoEntity? entity = getEntity.Value;

        if (entity is null || getEntity.Result is NotFoundObjectResult)
        {
            return getEntity.Result!;
        }

        if (!entity.Filepaths.Any(path => path.Dimension == dimension))
        {
            string message = $"Photo {nameof(PhotoEntity)} (#{photoId}) did not have a {dimension} {nameof(Dimension)}.";
            await logging
                .Action(nameof(GetSinglePhoto))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new Photo(entity, dimension);
    }

    /// <summary>
    /// Get the <see cref="Reception.Models.Photo"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<Photo>> GetSinglePhoto(string slug, Dimension dimension = Dimension.SOURCE)
    {
        var getEntity = await this.GetPhotoEntity(slug);
        PhotoEntity? entity = getEntity.Value;

        if (entity is null || getEntity.Result is NotFoundObjectResult)
        {
            return getEntity.Result!;
        }

        if (!entity.Filepaths.Any(path => path.Dimension == dimension))
        {
            string message = $"Photo {nameof(PhotoEntity)} ('{slug}') did not have a {dimension} {nameof(Dimension)}.";
            await logging
                .Action(nameof(GetSinglePhoto))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new Photo(entity, dimension);
    }


    /// <summary>
    /// Get the <see cref="PhotoCollection"/> with Primary Key '<paramref ref="photoId"/>'
    /// </summary>
    public async Task<ActionResult<PhotoCollection>> GetPhoto(int photoId)
    {
        if (photoId <= 0) {
            throw new ArgumentException($"Parameter {nameof(photoId)} has to be a non-zero positive integer!", nameof(photoId));
        }

        PhotoEntity? photo = await db.Photos.FindAsync(photoId);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} matching the given {nameof(photoId)} #{photoId}.";
            await logging
                .Action(nameof(GetPhoto))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (photo.Filepaths is null || photo.Filepaths.Count == 0)
        {
            // Load missing navigation entries.
            foreach(var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded) {
                    await navigation.LoadAsync();
                }
            }
        }

        if (!photo.Filepaths!.Any(path => path.Dimension == Dimension.SOURCE))
        {
            string message = $"{nameof(PhotoEntity)} '{photo.Slug}' didn't have a {Dimension.SOURCE} {nameof(Dimension)}";
            // throw new Exception(message);

            await logging
                .Action(nameof(GetPhoto))
                .InternalWarning(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new PhotoCollection(photo);
    }

    /// <summary>
    /// Get the <see cref="PhotoCollection"/> with Slug '<paramref ref="slug"/>' (string)
    /// </summary>
    public async Task<ActionResult<PhotoCollection>> GetPhoto(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        PhotoEntity? photo = await db.Photos
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Tags)
            .FirstOrDefaultAsync(photo => photo.Slug == slug);

        if (photo is null)
        {
            string message = $"Failed to find a {nameof(PhotoEntity)} matching the given {nameof(slug)} '{slug}'.";
            await logging
                .Action(nameof(GetPhoto))
                .LogDebug(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        if (photo.Filepaths is null || photo.Filepaths.Count == 0)
        {
            // Load missing navigation entries.
            foreach(var navigation in db.Entry(photo).Navigations)
            {
                if (!navigation.IsLoaded) {
                    await navigation.LoadAsync();
                }
            }
        }

        if (!photo.Filepaths!.Any(path => path.Dimension == Dimension.SOURCE))
        {
            string message = $"{nameof(PhotoEntity)} '{photo.Slug}' didn't have a {Dimension.SOURCE} {nameof(Dimension)}";
            // throw new Exception(message);

            await logging
                .Action(nameof(GetPhoto))
                .InternalWarning(message)
                .SaveAsync();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        return new PhotoCollection(photo);
    }
    #endregion


    #region Get many photos.
    /// <summary>
    /// Get all <see cref="Reception.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public Task<ActionResult<IEnumerable<Photo>>> GetSingles(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetSingles(filtering);
    }

    /// <summary>
    /// Get all <see cref="Reception.Models.Photo"/> instances matching a wide range of optional filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<Photo>>> GetSingles(FilterPhotosOptions filter)
    {
        filter.Dimension ??= Dimension.SOURCE;

        IQueryable<PhotoEntity> photoQuery = db.Photos
            .OrderByDescending(photo => photo.CreatedAt)
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Tags)
            .Where(photo => photo.Filepaths.Any(path => path.Dimension == filter.Dimension));

        // Filtering
        if (!string.IsNullOrWhiteSpace(filter.Slug))
        {
            photoQuery = photoQuery
                .Where(photo => photo.Slug.StartsWith(filter.Slug) || photo.Slug.EndsWith(filter.Slug));
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            photoQuery = photoQuery
                .Where(photo => !string.IsNullOrWhiteSpace(photo.Title))
                .Where(photo => photo.Title!.StartsWith(filter.Title) || photo.Title.EndsWith(filter.Title));
        }

        if (filter.CreatedAt is not null)
        {
            if (filter.CreatedAt > DateTime.UtcNow) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedAt, $"Filter Parameter {nameof(filter.CreatedAt)} cannot exceed DateTime.UtcNow");
            }

            photoQuery = photoQuery
                .Where(photo => photo.CreatedAt >= filter.CreatedAt);
        }

        if (filter.CreatedBy is not null)
        {
            if (filter.CreatedBy <= 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedBy, $"Filter Parameter {nameof(filter.CreatedBy)} has to be a non-zero positive integer (User ID)!");
            }

            photoQuery = photoQuery
                .Where(photo => photo.CreatedBy == filter.CreatedBy);
        }

        // Pagination
        if (filter.Offset is not null)
        {
            if (filter.Offset < 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Offset, $"Pagination Parameter {nameof(filter.Offset)} has to be a positive integer!");
            }

            photoQuery = photoQuery.Skip(filter.Offset.Value);
        }
        if (filter.Limit is not null)
        {
            if (filter.Limit <= 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Limit, $"Pagination Parameter {nameof(filter.Limit)} has to be a non-zero positive integer!");
            }

            photoQuery = photoQuery.Take(filter.Limit.Value);
        }

        var getPhotos = await photoQuery.ToListAsync();
        var photos = getPhotos
            .Select(photo => new Photo(photo, filter.Dimension!.Value))
            .ToList();

        return photos;
    }


    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return GetPhotos(filtering);
    }

    /// <summary>
    /// Assemble an <see cref="IEnumerable{Reception.Models.PhotoCollection}"/> collection of Photos matching a wide range of optional
    /// filtering / pagination options (<seealso cref="FilterPhotosOptions"/>).
    /// </summary>
    public async Task<ActionResult<IEnumerable<PhotoCollection>>> GetPhotos(FilterPhotosOptions filter)
    {
        IQueryable<PhotoEntity> photoQuery = db.Photos
            .OrderByDescending(photo => photo.CreatedAt)
            .Include(photo => photo.Filepaths)
            .Include(photo => photo.Tags);

        // Filtering
        if (filter.Dimension is not null) {
            photoQuery = photoQuery
                .Where(photo => photo.Filepaths.Any(path => path.Dimension == filter.Dimension));
        }

        if (!string.IsNullOrWhiteSpace(filter.Slug))
        {
            photoQuery = photoQuery
                .Where(photo => photo.Slug.StartsWith(filter.Slug) || photo.Slug.EndsWith(filter.Slug));
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            photoQuery = photoQuery
                .Where(photo => !string.IsNullOrWhiteSpace(photo.Title))
                .Where(photo => photo.Title!.StartsWith(filter.Title) || photo.Title.EndsWith(filter.Title));
        }

        if (filter.CreatedAt is not null)
        {
            if (filter.CreatedAt > DateTime.UtcNow) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedAt, $"Filter Parameter {nameof(filter.CreatedAt)} cannot exceed DateTime.UtcNow");
            }

            photoQuery = photoQuery
                .Where(photo => photo.CreatedAt >= filter.CreatedAt);
        }

        if (filter.CreatedBy is not null)
        {
            if (filter.CreatedBy <= 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.CreatedBy, $"Filter Parameter {nameof(filter.CreatedBy)} has to be a non-zero positive integer (User ID)!");
            }

            photoQuery = photoQuery
                .Where(photo => photo.CreatedBy == filter.CreatedBy);
        }

        // Pagination
        if (filter.Offset is not null)
        {
            if (filter.Offset < 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Offset, $"Pagination Parameter {nameof(filter.Offset)} has to be a positive integer!");
            }

            photoQuery = photoQuery.Skip(filter.Offset.Value);
        }
        if (filter.Limit is not null)
        {
            if (filter.Limit <= 0) {
                throw new ArgumentOutOfRangeException(nameof(filter), filter.Limit, $"Pagination Parameter {nameof(filter.Limit)} has to be a non-zero positive integer!");
            }

            photoQuery = photoQuery.Take(filter.Limit.Value);
        }

        var getPhotos = await photoQuery.ToListAsync();
        var photos = getPhotos
            .Select(entity => new PhotoCollection(entity))
            .ToList();

        return photos;
    }
    #endregion


    #region Create / Store a photo blob.
    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="FilterPhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhoto(Action<FilterPhotosOptions> opts)
    {
        FilterPhotosOptions filtering = new();
        opts(filtering);

        return UploadPhoto(filtering);
    }

    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="FilterPhotosOptions"/> (<paramref name="details"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public async Task<ActionResult<IEnumerable<PhotoCollection>>> UploadPhoto(FilterPhotosOptions details)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(UploadPhoto)} Failed: No {nameof(HttpContext)} found.";
            await logging
                .Action(nameof(UploadPhoto))
                .InternalError(message)
                .SaveAsync();

            return new UnauthorizedObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        if (!MultipartHelper.IsMultipartContentType(httpContext.Request.ContentType))
        {
            string message = $"{nameof(UploadPhoto)} Failed: Request couldn't be processed, not a Multipart Formdata request.";
            await logging
                .Action(nameof(UploadPhoto))
                .ExternalError(message)
                .SaveAsync();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.Unauthorized.ToString() : message
            );
        }

        details.CreatedAt ??= DateTime.UtcNow;
        Account? user = null;

        if (MageAuthentication.IsAuthenticated(contextAccessor))
        {
            try {
                user = MageAuthentication.GetAccount(contextAccessor);
            }
            catch (Exception ex) {
                await logging
                    .Action(nameof(UploadPhoto))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => { opts.Exception = ex; })
                    .SaveAsync();
            }
        }

        var mediaTypeHeader = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType!);
        string boundary = MultipartHelper.GetBoundary(mediaTypeHeader, 70);

        var reader = new MultipartReader(boundary, httpContext.Request.Body);
        MultipartSection? section;

        List<PhotoCollection> photos = [];
        uint iteration = 0u;
        do
        {
            section = await reader.ReadNextSectionAsync();
            if (section is null) {
                break;
            }

            bool hasContentDisposition =
                ContentDispositionHeaderValue.TryParse(section?.ContentDisposition, out var contentDisposition);

            if (!hasContentDisposition || contentDisposition is null) {
                continue;
            }

            if (MultipartHelper.HasFileContentDisposition(contentDisposition))
            {
                PhotoEntity? newPhoto;
                try {
	                newPhoto = await UploadSinglePhoto(
	                	details,
	                	contentDisposition,
	               		section!,
	                  	user
	                );

	                if (newPhoto is null)
	                {
	                    await logging
							.Action(nameof(UploadPhoto))
							.InternalError($"Failed to create a {nameof(PhotoEntity)} using uploaded photo. {nameof(newPhoto)} was null.")
							.SaveAsync();
	                    continue;
	                }
                }
                catch (Exception ex)
                {
                	await logging
                 		.Action(nameof(UploadPhoto))
                   		.InternalError($"Failed to upload a photo. ({ex.GetType().Name}) " + ex.Message, opts => {
                     		opts.Exception = ex;
                        })
                     	.SaveAsync();
                    continue;
                }

                if (newPhoto.Id == default)
                {
                	// New photo needs to be uploaded to the database..
                    var createEntity = await CreatePhotoEntity(newPhoto);
                    newPhoto = createEntity.Value;

                    if (newPhoto is null || newPhoto.Id == default)
                    {
                        await logging
	                        .Action(nameof(UploadPhoto))
	                        .InternalError($"Failed to create a {nameof(PhotoEntity)} using uploaded photo '{(newPhoto?.Slug ?? "null")}'. Entity was null, or its Photo ID remained as 'default' post-saving to the database ({(newPhoto?.Id.ToString() ?? "null")}).")
	                        .SaveAsync();
                        continue;
                    }
                }

                if (!newPhoto!.Filepaths.Any(path => path.IsSource))
                {
	                await logging
                        .Action(nameof(UploadPhoto))
                        .InternalError($"No '{Dimension.SOURCE.ToString()}' {nameof(Filepath)} found in the newly uploaded/created {nameof(PhotoEntity)} instance '{newPhoto.Slug}' (#{newPhoto.Id}).")
                        .SaveAsync();
	                continue;
                }

                photos.Add(
                	new(newPhoto)
                );
            }
            // TODO! Maaaaaybe implement support for this in the future.
            /* else if (MultipartHelper.HasFormDataContentDisposition(contentDisposition))
            {
                ...
            } */
        }
        while (section is not null && ++iteration < 4096u);

        return new CreatedAtActionResult(null, null, null, photos);
    }

    /// <summary>
    /// Upload a new <see cref="PhotoEntity"/> (<seealso cref="Reception.Models.Entities.Photo"/>) by streaming it directly to disk.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="FilterPhotosOptions"/> (<paramref name="opts"/>) has been repurposed to serve as the details of the
    /// <see cref="Reception.Models.Entities.Photo"/> database entity.
    /// </remarks>
    /// <returns><see cref="PhotoEntity"/></returns>
    protected async Task<PhotoEntity> UploadSinglePhoto(
    	FilterPhotosOptions options,
    	ContentDispositionHeaderValue contentDisposition,
    	MultipartSection section,
    	Account? user = null
    ) {
	    string sourcePath = string.Empty;
	    string mediumPath = string.Empty;
	    string thumbnailPath = string.Empty;
	    string fileExtension = string.Empty;
	    string trustedFilename = string.Empty;

	    // Don't trust the file name sent by the client. To display the file name, HTML-encode the value.
	    // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0#upload-large-files-with-streaming
	    string? untrustedFilename = contentDisposition.FileName.Value;

	    if (string.IsNullOrWhiteSpace(untrustedFilename)) {
	        throw new NotImplementedException("Handle case of no filename"); // TODO: HANDLE
	    }

	    trustedFilename = WebUtility.HtmlEncode(untrustedFilename);
	    fileExtension = Path.GetExtension(trustedFilename).ToLowerInvariant();
		if (fileExtension.StartsWith('.') && fileExtension.Length > 1) {
            fileExtension = fileExtension[1..];
        }

        if (string.IsNullOrWhiteSpace(fileExtension) || !MimeVerifyer.SupportedExtensions.Contains(fileExtension))
	    {
	        throw new NotImplementedException("The extension is invalid.. discontinuing processing of the file"); // TODO: HANDLE
	    }

	    using MemoryStream memStream = new MemoryStream();
	    await section.Body.CopyToAsync(memStream);

	    if (memStream.Length <= 0)
	    {
	        throw new NotImplementedException("The file is empty."); // TODO: HANDLE
	    }
	    if (memStream.Length > MultipartHelper.FILE_SIZE_LIMIT)
	    {
	        throw new NotImplementedException("The file is too large ... discontinuing processing of the file"); // TODO: HANDLE
	    }
	    if (!MimeVerifyer.ValidateContentType(trustedFilename, fileExtension, memStream))
	    {
	        throw new NotImplementedException("Error validating file content type / extension. Name missmatch, unsupported filetype or signature missmatch?"); // TODO: HANDLE
	    }


	    sourcePath = GetCombinedPath(Dimension.SOURCE, options.CreatedAt);
	    mediumPath = GetCombinedPath(Dimension.MEDIUM, options.CreatedAt);
	    thumbnailPath = GetCombinedPath(Dimension.THUMBNAIL, options.CreatedAt);

	    try {
	        Directory.CreateDirectory(sourcePath);
	        Directory.CreateDirectory(mediumPath);
	        Directory.CreateDirectory(thumbnailPath);
	    }
	    catch (Exception ex) {
	        /* // Handle billions of different exceptions, maybe..
	        IOException
	        UnauthorizedAccessException
	        ArgumentException
	        ArgumentNullException
	        PathTooLongException
	        DirectoryNotFoundException
	        NotSupportedException
	        */
	        throw new NotImplementedException($"Handle directory create errors {nameof(UploadPhoto)} ({sourcePath}) " + ex.Message); // TODO: HANDLE
	    }

        // Handle potential name-conflicts on the path..
        string filename = trustedFilename;
        string fullPath = Path.Combine(sourcePath, filename);
        int extensionIndex = trustedFilename.LastIndexOf('.');
        int conflicts = 0;

        while (File.Exists(fullPath) && ++conflicts <= 4096)
        {
        	string appendix = "_copy";
        	if (conflicts > 1) {
         		appendix += "_" + (conflicts - 1);
            }

            filename = extensionIndex != -1
            	? trustedFilename.Insert(extensionIndex, appendix)
             	: (trustedFilename + appendix);

            fullPath = Path.Combine(sourcePath, filename);
        }


        // Saved! :)
        using var file = File.Create(fullPath);
	    await file.WriteAsync(memStream.ToArray());

		long filesize = file.Length;
        string filesizeFormatted = filesize.ToString();

        if (filesize < 8388608) { // Kilobytes..
            filesizeFormatted = $"{Math.Round((decimal)(filesize / 8192), 1)}kB";
        }
        else if (filesize >= 8589934592) { // Gigabytes..
        	filesizeFormatted = $"{Math.Round((decimal)(filesize / 8589934592), 3)}GB";
        }
        else { // Megabytes..
       		filesizeFormatted = $"{Math.Round((decimal)(filesize / 8388608), 2)}MB";
        }

        options.CreatedAt ??= DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(options.Slug))
        {
			// `filename` is only guaranteed unique *for a single date (24h)*.
			options.Slug = $"{options.CreatedAt.Value.ToShortDateString().Replace('/', '-')}-{filename}";

			extensionIndex = options.Slug.LastIndexOf('.');
			if (extensionIndex != -1) {
                options.Slug = options.Slug[..extensionIndex];
            }

            // Resolve possible (..yet, unlikely) ..conflicts/duplicate slugs:
			int count = await db.Photos.CountAsync(photo => photo.Slug == options.Slug);
			if (count > 0) {
				options.Slug += "_" + count;
			}
        }

        if (string.IsNullOrWhiteSpace(options.Title))
        {
            options.Title = trustedFilename;

            if (conflicts > 0) {
                options.Title += $" (#{conflicts})";
            }
        }

        logging
	        .Action(nameof(UploadPhoto))
	        .ExternalInformation($"Finished streaming file '{filename}' to '{sourcePath}'");

        StringBuilder formattedDescription = new();
        formattedDescription.Append("Uploaded ");
        formattedDescription.Append(options.CreatedAt.Value.Month switch
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
        formattedDescription.AppendFormat(" {0}", options.CreatedAt.Value.Year);
        formattedDescription.Append(", saved to ");
        formattedDescription.AppendFormat("'{0}' ", sourcePath);
        formattedDescription.AppendFormat("({0})", filesizeFormatted);

        if (conflicts > 0) {
        	formattedDescription.AppendFormat(". Potentially a copy of {0} other files.", conflicts);
        }

        // TODO! Replace DB Call here with some tags service..
        var year_tag = await db.Tags.FirstOrDefaultAsync(
        	tag => tag.Name == options.CreatedAt.Value.Year.ToString()
        );

        PhotoEntity photo = new() {
            Slug = options.Slug,
            Title = options.Title,
            Summary = options.Summary ?? $"{options.Title} - {filesizeFormatted}",
            Description = formattedDescription.ToString(),
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = options.CreatedAt.Value,
            CreatedBy = options.CreatedBy,
            Filepaths = [
                new() {
                    Filename = filename,
                    Path = sourcePath,
                    Dimension = Dimension.SOURCE,
                    Filesize = filesize,
                }
            ],
            Tags = [
                year_tag ?? new () {
                    Name = options.CreatedAt.Value.Year.ToString(),
                    Description = "Images uploaded during " + options.CreatedAt.Value.Year.ToString()
                }
            ]
        };

		if (user is not null) {
            photo.CreatedBy = user.Id;
        }

        if (filesize >= MultipartHelper.LARGE_FILE_THRESHOLD)
        {
	        // TODO! Replace DB Call here with some tags service..
	        var hd_tag = await db.Tags.FirstOrDefaultAsync(
	        	tag => tag.Name == MultipartHelper.LARGE_FILE_CATEGORY_SLUG
	        );

            photo.Tags.Add(new()
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
	        var sd_tag = await db.Tags.FirstOrDefaultAsync(
	        	tag => tag.Name == "Copy"
	        );

	        photo.Tags.Add(sd_tag ?? new()
	        {
		            Name = "Copy",
		            Description = "Image might be a copy of another, its filename conflicts with at least one other file uploaded around the same time."
	        });
        }

        return photo;
    }
    #endregion


    #region Create a Filepath entity.
    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Filepath"/> in the database.
    /// </summary>
    /// <remarks>
    /// <trong>Note:</strong> Assumes a size of <see cref="Reception.Models.Entities.Dimension.SOURCE"/>.
    /// </remarks>
    public Task<ActionResult<Filepath>> CreateFilepathEntity(string filename, int photoId) =>
        CreateFilepathEntity(Dimension.SOURCE, filename, photoId);

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public Task<ActionResult<Filepath>> CreateFilepathEntity(Dimension dimension, string filename, int photoId)
    {
        if (string.IsNullOrWhiteSpace(filename)) {
            throw new NotImplementedException("Filename null or empty"); // TODO: HANDLE
        }

        return CreateFilepathEntity(new Filepath() {
            Dimension = dimension,
            Filename = filename,
            PhotoId = photoId
        });
    }

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Filepath"/> in the database.
    /// </summary>
    /// <remarks>
    /// <trong>Note:</strong> Assumes a size of <see cref="Reception.Models.Entities.Dimension.SOURCE"/>.
    /// </remarks>
    public async Task<ActionResult<Filepath[]>> CreateFilepathEntity(PhotoEntity photo, string? filename)
    {
        ArgumentNullException.ThrowIfNull(photo.Filepaths, nameof(photo.Filepaths));
        List<Filepath> paths = [];

        foreach(Filepath path in photo.Filepaths)
        {
            if (!string.IsNullOrWhiteSpace(filename)) {
                path.Filename = filename;
            }
            else if (string.IsNullOrWhiteSpace(path.Filename)) {
                continue; // Skip `path` if it has no Filename
            }

            if (path.PhotoId <= 0 || path.PhotoId != photo.Id) {
                path.PhotoId = photo.Id;
            }

            path.Photo ??= photo;

            var createFilepath = await CreateFilepathEntity(path);
            if (createFilepath.Value is null) {
                return createFilepath.Result!;
            }

            paths.Add(createFilepath.Value);
        }

        return paths.ToArray();
    }

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Filepath"/> in the database.
    /// </summary>
    public async Task<ActionResult<Filepath>> CreateFilepathEntity(Filepath path)
    {
        PhotoEntity? photo = path.Photo;
        if (photo is null)
        {
            if (path.PhotoId is <= 0)
            {
                throw new NotImplementedException("path.PhotoId less than equal 0"); // TODO: HANDLE
            }

            photo = await db.Photos.FindAsync(path.PhotoId);

            if (photo is null)
            {
                throw new NotImplementedException("Photo is null"); // TODO: HANDLE
            }
        }

        path.Path = GetCombinedPath(path.Dimension ?? Dimension.SOURCE, photo.CreatedAt);
        Directory.CreateDirectory(path.Path); // TODO - LOG & ERROR HANDLE

        try {
            db.Add(path);
            await db.SaveChangesAsync();
        }
        catch(Exception ex) {
            throw new NotImplementedException($"Handle db errors {nameof(CreateFilepathEntity)} " + ex.Message); // TODO: HANDLE
        }

        return path;
    }
    #endregion


    #region Create a photo entity.
    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public Task<ActionResult<PhotoEntity>> CreatePhotoEntity(MutatePhoto mut) =>
        CreatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Create a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> CreatePhotoEntity(PhotoEntity entity)
    {
    	if (string.IsNullOrWhiteSpace(entity.Slug))
     	{
            int sourceIndex = entity.Filepaths
              .ToList().FindIndex(path => path.IsSource);

            if (sourceIndex != -1)
            {
                string filename = entity.Filepaths.ElementAt(sourceIndex).Filename;
                entity.Slug = WebUtility.HtmlEncode(filename)
                	.ToLower()
                	.Replace(" ", "-")
                	.Replace(".", "-");
            }
            else if (string.IsNullOrWhiteSpace(entity.Title))
            {
            	string message = $"Can't save bad {nameof(PhotoEntity)} (#{entity.Id}) to database, entity has no '{Dimension.SOURCE.ToString()}' {nameof(Filepath)} and both '{nameof(PhotoEntity.Slug)}' & '{nameof(PhotoEntity.Title)}' are null/omitted!";
	            await logging
	               .Action(nameof(CreatePhotoEntity))
	               .ExternalWarning(message)
	               .SaveAsync();

				return new BadRequestObjectResult(message);
            }
            else {
	            entity.Slug = WebUtility.HtmlEncode(entity.Title)
	            	.ToLower()
	            	.Replace(" ", "-")
	            	.Replace(".", "-");

                bool exists = await db.Photos.AnyAsync(photo => photo.Slug == entity.Slug);
                if (exists)
                {
	                string message = $"{nameof(PhotoEntity)} with unique '{entity.Slug}' already exists!";
	                await logging
	                   .Action(nameof(CreatePhotoEntity))
	                   .ExternalWarning(message)
	                   .SaveAsync();

	                return new ObjectResult(message) {
	                    StatusCode = StatusCodes.Status409Conflict
	                };
                }
            }
     	}

        try {
        	db.Add(entity);
            await db.SaveChangesAsync();

            if (entity.Id == default) {
	            await db.Entry(entity).ReloadAsync();
            }
        }
        catch(DbUpdateConcurrencyException concurrencyException)
        {
            string message = string.Empty;
            bool exists = await db.Photos.ContainsAsync(entity);

            if (exists)
            {
                message = $"{nameof(PhotoEntity)} '{entity.Slug}' (#{entity.Id}) already exists!";
                await logging
                   .Action(nameof(CreatePhotoEntity))
                   .InternalError(message, opts => {
                       opts.Exception = concurrencyException;
                   })
                   .SaveAsync();

                return new ObjectResult(message) {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            message = $"Cought a {nameof(DbUpdateConcurrencyException)} while attempting to save '{entity.Slug}' (#{entity.Id}) to database! ";
            await logging
               .Action(nameof(CreatePhotoEntity))
               .InternalError(message + concurrencyException.Message, opts => {
                    opts.Exception = concurrencyException;
                })
				.SaveAsync();

            return new ObjectResult(message) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch(DbUpdateException updateException)
        {
            string message = $"Cought a {updateException.GetType().Name} while attempting to save '{entity.Slug}' (#{entity.Id}) to database! ";
            await logging
               .Action(nameof(CreatePhotoEntity))
               .InternalError(message + updateException.Message, opts => {
                    opts.Exception = updateException;
                })
				.SaveAsync();

            return new ObjectResult(message) {
            	StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return entity;
    }
    #endregion


    #region Update a photo entity.
    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(int photoId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(MutatePhoto mut) =>
        UpdatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Updates a <see cref="Reception.Models.Entities.Photo"/> in the database.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> UpdatePhotoEntity(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }
    #endregion


    #region Delete a photo completely (blob, filepaths & photo)
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> DeletePhoto(int photoId)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..identified by PK <paramref name="photoId"/>) ..completely,
    /// removing both the blob on-disk, and its database entry.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> DeletePhoto(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }
    #endregion


    #region Delete a blob from disk
    /// <summary>
    /// Deletes the blob of a <see cref="Reception.Models.Entities.Photo"/> from disk.
    /// </summary>
    public async Task<ActionResult<PhotoEntity>> DeletePhotoBlob(Filepath entity)
    {
        throw new NotImplementedException();
    }
    #endregion


    #region Delete a photo entities from the database
    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public Task<ActionResult<PhotoEntity>> DeletePhotoEntity(MutatePhoto mut) =>
        UpdatePhotoEntity((PhotoEntity)mut);

    /// <summary>
    /// Deletes a <see cref="Reception.Models.Entities.Photo"/> (..and associated <see cref="Reception.Models.Entities.Filepath"/> entities) ..from the database.
    /// </summary>
    /// <remarks>
    /// <strong>Note:</strong> Since this does *not* delete the blob on-disk, be mindful you don't leave anything dangling..
    /// </remarks>
    public async Task<ActionResult<PhotoEntity>> DeletePhotoEntity(PhotoEntity entity)
    {
        throw new NotImplementedException();
    }
    #endregion
}
