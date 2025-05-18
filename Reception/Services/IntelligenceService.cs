using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;
using Microsoft.AspNetCore.Mvc;
using Reception.Utilities;

namespace Reception.Services;

public class IntelligenceService(
    ILoggingService<IntelligenceService> logging,
    IPhotoService photoService,
    ITagService tagService,
    IBlobService blobs
) : IIntelligenceService
{
    /// <summary>
    /// Ping Ollama over HTTP w/ <see cref="HttpClient"/>
    /// </summary>
    protected async Task<HttpStatusCode> PingOllama(CancellationToken? token = null) {
        if (token.HasValue) {
            token.Value.ThrowIfCancellationRequested();
        }

        string url = Program.SecretaryUrl;
        ArgumentNullException.ThrowIfNullOrWhiteSpace(url);
        using HttpClient client = new HttpClient();
        var response = await client.GetAsync(url, token ?? default);
        return response.StatusCode;
    }

    /// <summary>
    /// Reach out to Ollama over HTTP w/ <see cref="HttpClient"/>
    /// </summary>
    protected async Task<(OllamaResponse?, OllamaResponse.Analysis?)> Ollama(OllamaRequest request, CancellationToken? token = null) {
        string url = Program.SecretaryUrl;
        ArgumentNullException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(request.Model);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(request.Prompt);

        using HttpClient client = new HttpClient();
        var response = await client.PostAsJsonAsync<OllamaRequest>(url + "/api/generate", request, token ?? default);

        if (token.HasValue) {
            token.Value.ThrowIfCancellationRequested();
        }

        var content = await response.Content.ReadAsStringAsync();

        content = content.Trim();
        if (!content.IsNormalized()) {
            content = content.Normalize();
        }

        content = content.Replace("```json", string.Empty);
        content = content.Replace("```", string.Empty);
        // content = content.Replace("\\n", string.Empty);

        Console.WriteLine(content);
        JsonSerializerOptions opts = new() {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate
        };

        OllamaResponse? ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(content, opts);
        OllamaResponse.Analysis? ollamaAnalysis = null;
        if (ollamaResponse is not null &&
            ollamaResponse.Response is not null
        ) {
            ollamaAnalysis = JsonSerializer.Deserialize<OllamaResponse.Analysis>(ollamaResponse.Response, opts);
        }

        return (ollamaResponse, ollamaAnalysis);
        /*
        content = content.Replace("\\\"", "\"");
        content = content.Replace("\"{", string.Empty);
        content = content.Replace("\" {", string.Empty);
        content = content.Replace("}\"", string.Empty);
        content = content.Replace("} \"", string.Empty);
        */
        // return await content.ReadFromJsonAsync<OllamaResponse>();
    }


    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public async Task<ActionResult<OllamaAnalysis>> InferSourceImage(int photoId, CancellationToken? token = null) {
        var getPhoto = await photoService.GetPhoto(photoId);
        Photo? entity = getPhoto.Value;

        if (entity is null) {
            return getPhoto.Result!;
        }

        return await this.InferSourceImage(entity, token);
    }

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public Task<ActionResult<OllamaAnalysis>> InferSourceImage(Photo entity, CancellationToken? token = null) =>
        this.View(Dimension.SOURCE, entity, token);


    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public async Task<ActionResult<OllamaAnalysis>> InferMediumImage(int photoId, CancellationToken? token = null) {
        var getPhoto = await photoService.GetPhoto(photoId);
        Photo? entity = getPhoto.Value;

        if (entity is null) {
            return getPhoto.Result!;
        }

        return await this.InferMediumImage(entity, token);
    }

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public Task<ActionResult<OllamaAnalysis>> InferMediumImage(Photo entity, CancellationToken? token = null) =>
        this.View(Dimension.MEDIUM, entity, token);


    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public async Task<ActionResult<OllamaAnalysis>> InferThumbnailImage(int photoId, CancellationToken? token = null) {
        var getPhoto = await photoService.GetPhoto(photoId);
        Photo? entity = getPhoto.Value;

        if (entity is null) {
            return getPhoto.Result!;
        }

        return await this.InferThumbnailImage(entity, token);
    }

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="Photo"/> (blob)
    /// </summary>
    public Task<ActionResult<OllamaAnalysis>> InferThumbnailImage(Photo entity, CancellationToken? token = null) =>
        this.View(Dimension.THUMBNAIL, entity, token);


    /// <summary>
    /// View the <see cref="Photo"/> (<paramref name="dimension"/>, blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public async Task<ActionResult<OllamaAnalysis>> View(Dimension dimension, Photo entity, CancellationToken? token = null) {
        if (token.HasValue) {
            token.Value.ThrowIfCancellationRequested();
        }

        HttpStatusCode ollamaStatus;
        try
        {
            ollamaStatus = await this.PingOllama(token);
            // Simple validation for now..
            if ((int)ollamaStatus < StatusCodes.Status200OK && (int)ollamaStatus > StatusCodes.Status308PermanentRedirect) {
                return new ObjectResult($"Failed to reach Ollama, status '{ollamaStatus}'")
                {
                    StatusCode = StatusCodes.Status503ServiceUnavailable
                };
            }
        }
        catch (Exception ex)
        {
            string message = $"{nameof(PingOllama)}(..) threw an error! '{ex.Message}'";
            logging
                .Action($"{nameof(PingOllama)}/{nameof(View)}")
                .InternalError(message, opts => {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        var getBlob = await blobs.GetBlobAsync(entity, dimension);
        if (getBlob is not FileStreamResult) {
            string message = $"{nameof(IBlobService.GetBlob)}(..) failed to load blob! '{getBlob.GetType().FullName}'";
            logging
                .Action($"{nameof(PingOllama)}/{nameof(View)}")
                .InternalError(message)
                .LogAndEnqueue();

            return getBlob;
        }

        using Stream base64Stream = ((FileStreamResult)getBlob).FileStream;
        byte[] buffer = new byte[base64Stream.Length];
        await base64Stream.ReadAsync(buffer, 0, buffer.Length);

        // Creating the request we're gonna be storing the blob we just loaded inside of..
        OllamaRequest request = new() {
            // Name of the model to run (required)
            Model = "llava_7b_json",
            // Text prompt for generation (required)
            Prompt = @"You are a tool used to extract information ([i]) from images.

            Information ([i]) definition:
            <Context>
            This tool will be used by a family of three, to help organize, categorize and label their collectively taken & stored images on their home server.
            Dad: Max (Maxylan), the greybeard who made the tool
            Mum: Ronja (Skai), the beauty who ensures the house doesn't go up in flames
            Son: Leo, the latest (and cutest!) addition to the family!
            </Context>

            Information ([i]) definition:
            <Definition>
            The term 'information' in this context means we are looking for a good mixture/blend of details that index the image to a user.
            This means repition is heavily discouraged, as is fixating on a specific topic such as the people or the weather.
            The greater the 'variety' of relevant topics you might find, the better the final indexing will be!
            </Definition>

            Your most important ground rules, which must not be violated under any circumstances, are as follows:
            <Rules>
            1. Your final response is a valid JSON object.
            2. The final JSON object always contains the following fields (..prefer `null` over empty field values)
                2a. 'summary' (string, 20-100 characters) - Brief, easily indexable (..but still human readable..) summary of your content analysis findings
                2b. 'description' (string, 80-400 characters) - A human-readable description of image contents.
                2c. 'tags' (string[], 4-16 items) - Array of single-word tags (strings) that index / categorize the image & its contents.
            3. Stay SFW (safe-for-work). Don't make up *unknown* people/names and/or topics.
            </Rules>

            Example Response - You're given a picture that seems to show the family playing in a park:
            <Example>
            {
                ""summary"": ""A picture of the entire family (Max, Ronja & Leo) together in the park on a sunny day."",
                ""description"": ""The entire family (Max, Ronja & Leo) together in the park, it's sunny outside and ..."",
                ""tags"": [""Max"", ""Ronja"", ""Leo"", ""Outdoors"", ""Park"", ""Sunny"", ...]
            }
            </Example>
            ",
            // Array of Base64-images as strings
            Images = [Convert.ToBase64String(buffer)],
            // Optional: stream back partial results
            Stream = false,
            // Optional: number of tokens to predict
            // NumPredict
            // Optional: top_k sampling parameter
            TopK = 34,
            // Optional: top_p sampling parameter
            TopP = 0.92F,
            // Optional: temperature parameter for randomness
            Temperature = 0.69F,
            // Optional: penalty to reduce repetition
            RepeatPenalty = 1.25F,
            // Optional: a seed value for deterministic results
            Seed = 20240720,
            // Optional: list of stop strings to control generation stopping
            // Stop
            // Optional: extra custom options as key-value pairs
            // Options
        };
        buffer = []; // Manual de-allocation, unsure if necessary tho..

        (OllamaResponse?, OllamaResponse.Analysis?) response;
        try
        {
            response = await this.Ollama(request, token);
        }
        catch (Exception ex)
        {
            string message = $"{nameof(Ollama)}(..) threw an error! '{ex.Message}'";
            logging
                .Action($"{nameof(Ollama)}/{nameof(InferSourceImage)}")
                .InternalError(message, opts => {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        if (response.Item1 is null || response.Item2 is null) {
            return new ObjectResult($"Failed to read response from Ollama (null), status '{ollamaStatus}'")
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
        }

        return new OllamaAnalysis() {
            // The generated text content
            Response = response.Item2,
            // Indicates whether generation has completed
            Done = response.Item1.Done,
            // Echoed back model name that was used
            Model = response.Item1.Model,
            // Timestamp (or equivalent string) when generation occurred
            CreatedAt = response.Item1.CreatedAt,
            // An identifier for the log or session (if provided)
            LogId = response.Item1.LogId,
            // Optional: error message if something went wrong
            Error = response.Item1.Error
        };
    }


    /// <summary>
    /// Deliver a <paramref name="prompt"/> to a <paramref name="model"/> (string)
    /// </summary>
    public Task<ActionResult<OllamaResponse>> Chat(string prompt, string model, CancellationToken? token = null) {
        throw new NotImplementedException(nameof(Chat) + " is not implemented yet!");
    }

    #region AI Analysis
    /// <summary>
    /// tbd
    /// </summary>
    /// <remarks>
    /// tbd
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public async Task<ActionResult<Photo>> ApplyPhotoAnalysis(
        int photoId,
        ActionResult<OllamaAnalysis> imageAnalysis,
        CancellationToken cancellationToken
    ) {
        var getPhoto = await photoService.GetPhoto(photoId);
        Photo? entity = getPhoto.Value;

        if (entity is null) {
            return getPhoto;
        }

        return await this.ApplyPhotoAnalysis(entity, imageAnalysis, cancellationToken);
    }

    /// <summary>
    /// tbd
    /// </summary>
    /// <remarks>
    /// tbd
    /// </remarks>
    /// <returns><see cref="PhotoCollection"/></returns>
    public async Task<ActionResult<Photo>> ApplyPhotoAnalysis(
        Photo photo,
        ActionResult<OllamaAnalysis> imageAnalysis,
        CancellationToken cancellationToken
    ) {
        cancellationToken.ThrowIfCancellationRequested();
        OllamaAnalysis? analysis = imageAnalysis.Value;

        if (analysis is null) {
            return imageAnalysis.Result!;
        }

        if (analysis.Response is null) {
            string message = $"Failed to analyze photo '{photo.Slug}' (#{photo.Id}), could not parse the LLM's 'response': '{analysis.Response}'";
            logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalWarning(message)
                .LogAndEnqueue();

            return photo;
        }

        // Since we're dealing with concurrency here, start with a "does this even exist?"-sanity-check..
        // P.P.S - This has been moved to a service which no longer has an injected `DbContext`..
        /* bool photoStillExists = await db.Photos.AnyAsync(entity => entity.Id == photo.Id, cancellationToken);
        if (!photoStillExists) {
            string message = $"Failed to apply analysis of photo '{photo.Slug}' (#{photo.Id}) no longer exists?";
            await logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalWarning(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        try {
            if (analysis.Response is null) {
                string message = $"Failed to analyze photo '{photo.Slug}' (#{photo.Id}), could not parse the LLM's 'response': '{analysis.Response}'";
                await logging
                    .Action(nameof(ApplyPhotoAnalysis))
                    .InternalWarning(message)
                    .LogAndEnqueue();

                return photo;
            }
        }
        catch (JsonException ex) {
            string message = $"Failed to analyze photo '{photo.Slug}' (#{photo.Id}), cought a '{nameof(JsonException)}' attempting to parse the LLM's 'response': '{analysis.Response}'";
            await logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalError(message, opts => {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return photo;
        } */

        // Since we're dealing with concurrency here, ensure we're operating on the latest values found in the database..
        // P.P.P.S - This has been moved to a service which no longer has an injected `DbContext`..
        /* try {
            await db.Photos.Entry(photo).ReloadAsync(cancellationToken);
        }
        catch (Exception ex) {
            string message = $"Failed to analyze photo '{photo.Slug}' (#{photo.Id}), cought a '{ex.GetType().FullName}' attempting to fetch latest values of the photo from the database.";
            await logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalError(message, opts => {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return photo;
        } */

        if (!string.IsNullOrWhiteSpace(analysis.Response.Summary)) {
            int maxLength = 255 - (photo.Summary?.Length ?? 0);

            if (!string.IsNullOrWhiteSpace(photo.Summary)) {
                analysis.Response.Summary += " - " + photo.Summary;
            }

            if (maxLength > 3) {
                photo.Summary = analysis.Response.Summary
                    .Replace("&", "and")
                    .Replace("|", "or")
                    .Replace(@"\", string.Empty)
                    .Trim()
                    .Normalize()
                    .Subsmart(0, maxLength);
            }
        }
        else {
            logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalWarning("Failed to get 'summary' from photo analysis.")
                .LogAndEnqueue();
        }

        if (!string.IsNullOrWhiteSpace(analysis.Response.Description)) {
            int maxLength = 32767 - (photo.Description?.Length ?? 0);

            if (!string.IsNullOrWhiteSpace(photo.Description)) {
                analysis.Response.Description += " - " + photo.Description;
            }

            if (maxLength > 3) {
                photo.Description = analysis.Response.Description
                    .Replace("&", "and")
                    .Replace("|", "or")
                    .Replace(@"\", string.Empty)
                    .Trim()
                    .Normalize()
                    .Subsmart(0, maxLength);
            }
        }
        else {
            logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalWarning("Failed to get 'description' from photo analysis.")
                .LogAndEnqueue();
        }

        IEnumerable<Tag> tags = [];
        if (analysis.Response.Tags is not null)
        {
            var sanitizedTags = analysis.Response.Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(
                    tag => tag
                        .Trim()
                        .Normalize()
                        .Replace(" ", "_")
                        .Replace("&", "and")
                        .Replace("|", "or")
                        .Replace(@"\", string.Empty)
                        .Subsmart(0, 127)
                )
                .Where(tag => photo.Tags.Any(relation => relation.Tag.Name == tag));

            var sanitizeAndCreateTags = await tagService.CreateTags(sanitizedTags);
            if (sanitizeAndCreateTags.Value is not null)
            {
                tags = sanitizeAndCreateTags.Value;
            }
        }
        else {
            logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalWarning("Failed to get 'tags' from photo analysis.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        try {
            await photoService.UpdatePhoto(new MutatePhoto() {
                Id = photo.Id,
                Slug = photo.Slug,
                Title = photo.Title,
                Summary = photo.Summary,
                Description = photo.Description,
                /* UploadedBy = photo.UploadedBy,
                UploadedAt = photo.UploadedAt,
                UpdatedAt = photo.UpdatedAt,
                CreatedAt = photo.CreatedAt, */
                // Navigation
                /* Accounts = photo.Accounts,
                ThumbnailForAlbums = photo.ThumbnailForAlbums,
                Filepaths = photo.Filepaths,
                Links = photo.Links,
                UploadedByNavigation = photo.UploadedByNavigation,
                AlbumsNavigation = photo.AlbumsNavigation, */
                Tags = tags
            });

            logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalInformation($"{nameof(Photo)} '{photo.Slug}' (#{photo.Id}) was just analyzed.")
                .LogAndEnqueue();
        }
        catch (Exception ex) {
            string message = $"Failed to save post-analysis updates to photo '{photo.Slug}' (#{photo.Id}). {ex.GetType().BaseType} '{ex.Message}'";
            logging
                .Action(nameof(ApplyPhotoAnalysis))
                .InternalWarning(message)
                .LogAndEnqueue();
        }

        return photo;
    }
    #endregion
}
