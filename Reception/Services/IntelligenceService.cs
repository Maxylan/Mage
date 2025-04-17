using Reception.Models;
using Reception.Models.Entities;
using Reception.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Reception.Services;

public class IntelligenceService(
    ILoggingService logging,
    IPhotoService photos,
    IBlobService blobs
) : IIntelligenceService
{
    /// <summary>
    /// Ping Ollama over HTTP w/ <see cref="HttpClient"/>
    /// </summary>
    protected async Task<HttpStatusCode> PingOllama() {
        string url = Program.SecretaryUrl;
        ArgumentNullException.ThrowIfNullOrWhiteSpace(url);
        using HttpClient client = new HttpClient();
        var response = await client.GetAsync(url);
        return response.StatusCode;
    }

    /// <summary>
    /// Reach out to Ollama over HTTP w/ <see cref="HttpClient"/>
    /// </summary>
    protected async Task<OllamaResponse?> Ollama(OllamaRequest request) {
        string url = Program.SecretaryUrl;
        ArgumentNullException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(request.Model);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(request.Prompt);

        using HttpClient client = new HttpClient();
        var response = await client.PostAsJsonAsync<OllamaRequest>(url + "/api/generate", request);

        return await response.Content.ReadFromJsonAsync<OllamaResponse>();
    }

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public async Task<ActionResult<OllamaResponse>> InferSourceImage(int photoId) {
        var getPhoto = await photos.GetPhotoEntity(photoId);
        PhotoEntity? entity = getPhoto.Value;

        if (entity is null) {
            return getPhoto.Result!;
        }

        return await this.InferSourceImage(entity);
    }

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Source'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public Task<ActionResult<OllamaResponse>> InferSourceImage(PhotoEntity entity) =>
        this.View(Dimension.SOURCE, entity);


    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public async Task<ActionResult<OllamaResponse>> InferMediumImage(int photoId) {
        var getPhoto = await photos.GetPhotoEntity(photoId);
        PhotoEntity? entity = getPhoto.Value;

        if (entity is null) {
            return getPhoto.Result!;
        }

        return await this.InferMediumImage(entity);
    }

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Medium'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public Task<ActionResult<OllamaResponse>> InferMediumImage(PhotoEntity entity) =>
        this.View(Dimension.MEDIUM, entity);


    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public async Task<ActionResult<OllamaResponse>> InferThumbnailImage(int photoId) {
        var getPhoto = await photos.GetPhotoEntity(photoId);
        PhotoEntity? entity = getPhoto.Value;

        if (entity is null) {
            return getPhoto.Result!;
        }

        return await this.InferThumbnailImage(entity);
    }

    /// <summary>
    /// Reach out to Ollama to infer the contents of a 'Thumbnail'-quality <see cref="PhotoEntity"/> (blob)
    /// </summary>
    public Task<ActionResult<OllamaResponse>> InferThumbnailImage(PhotoEntity entity) =>
        this.View(Dimension.THUMBNAIL, entity);


    /// <summary>
    /// View the <see cref="PhotoEntity"/> (<paramref name="dimension"/>, blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// </remarks>
    public async Task<ActionResult<OllamaResponse>> View(Dimension dimension, PhotoEntity entity) {
        HttpStatusCode ollamaStatus;
        try
        {
            ollamaStatus = await this.PingOllama();
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
            await logging
                .Action($"{nameof(PingOllama)}/{nameof(InferSourceImage)}")
                .InternalError(message, opts => {
                    opts.Exception = ex;
                })
                .SaveAsync();

            return new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        string encodedImage;
        blobs.GetBlob(dimension, entity)

        OllamaResponse? response;
        try
        {
            response = await this.Ollama(new() {
                // Name of the model to run (required)
                Model = "llava_json",
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
                3. Stay objective & SFW (safe-for-work). Don't make up unknown people/names and/or topics.
                </Rules>

                Example Response - You're given a picture that seems to show the family playing in a park:
                <Example>
                {
                    ""summary"": ""A picture of the entire family (Max, Ronja & Leo) together in the park on a sunny day."",
                    ""description"": ""The entire family (Max, Ronja & Leo) together in the park, it's sunny outside and...<continuation/>"",
                    ""tags"": [""Max"", ""Ronja"", ""Leo"", ""Outdoors"", ""Park"", ""Sunny"", ...<continuation/>]
                }
                </Example>
                ",
                // Array of Base64-images as strings
                Images = [encodedImage],
                // Optional: stream back partial results
                Stream = false,
                // Optional: number of tokens to predict
                // NumPredict
                // Optional: top_k sampling parameter
                // TopK
                // Optional: top_p sampling parameter
                // TopP
                // Optional: temperature parameter for randomness
                Temperature = 0.78F,
                // Optional: penalty to reduce repetition
                RepeatPenalty = 1.24F,
                // Optional: a seed value for deterministic results
                Seed = 20240720,
                // Optional: list of stop strings to control generation stopping
                // Stop
                // Optional: extra custom options as key-value pairs
                // Options
            });
        }
        catch (Exception ex)
        {
            string message = $"{nameof(Ollama)}(..) threw an error! '{ex.Message}'";
            await logging
                .Action($"{nameof(Ollama)}/{nameof(InferSourceImage)}")
                .InternalError(message, opts => {
                    opts.Exception = ex;
                })
                .SaveAsync();

            return new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        if (response is null) {
            return new ObjectResult($"Failed to read response from Ollama (null), status '{ollamaStatus}'")
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
        }

        return response;
    }


    /// <summary>
    /// Deliver a <paramref name="prompt"/> to a <paramref name="model"/> (string)
    /// </summary>
    public async Task<ActionResult<OllamaResponse>> Chat(string prompt, string model) {
        throw new NotImplementedException(nameof(Chat) + " is not implemented yet!");
    }
}
