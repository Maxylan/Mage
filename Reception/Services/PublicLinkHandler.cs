using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Middleware.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Services;

public class PublicLinkHandler(
    ILoggingService<PublicLinkHandler> logging,
    IPublicLinkService linkService
) : IPublicLinkHandler
{
    /// <summary>
    /// Get the <see cref="PublicLink"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public async Task<ActionResult<PublicLinkDTO>> GetLink(int linkId)
    {
        if (linkId <= 0)
        {
            string message = $"Parameter {nameof(linkId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.GetLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getLink = await linkService.GetLink(linkId);
        if (getLink.Value is null)
        {
            string message = $"Failed to get {nameof(PublicLink)} with ID #{nameof(linkId)}!";
            logging
                .Action(nameof(PublicLinkHandler.GetLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getLink.Result!;
        }

        return (PublicLinkDTO)getLink.Value;
    }
    /// <summary>
    /// Get the <see cref="PublicLink"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult<PublicLinkDTO>> GetLinkByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            string message = $"Parameter '{nameof(code)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.GetLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getLink = await linkService.GetLinkByCode(code);
        if (getLink.Value is null)
        {
            string message = $"Failed to get {nameof(PublicLink)} with GUID '{nameof(code)}'!";
            logging
                .Action(nameof(PublicLinkHandler.GetLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getLink.Result!;
        }

        return (PublicLinkDTO)getLink.Value;
    }

    /// <summary>
    /// Get all <see cref="PublicLink"/> entries.
    /// </summary>
    public async Task<ActionResult<IEnumerable<PublicLinkDTO>>> GetLinks(int limit = 99, int offset = 0)
    {
        if (limit <= 0)
        {
            string message = $"Parameter '{nameof(limit)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.GetLinks))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (offset < 0)
        {
            string message = $"Parameter '{nameof(offset)}' has to be a positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.GetLinks))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getLinks = await linkService.GetLinks(limit, offset);
        var publicLinks = getLinks.Value;

        if (publicLinks is null)
        {
            string message = $"Failed to get all {nameof(PublicLink)}(s)!";
            logging
                .Action(nameof(PublicLinkHandler.GetLinks))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getLinks.Result!;
        }

        return publicLinks
            .Select(link => (PublicLinkDTO)link)
            .ToArray();
    }

    /// <summary>
    /// Create a <see cref="PublicLink"/> to the <see cref="Photo"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public virtual Task<ActionResult<PublicLinkDTO>> CreateLink(int photoId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return CreateLink(photoId, mutationOptions);
    }
    /// <summary>
    /// Create a <see cref="PublicLink"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public async Task<ActionResult<PublicLinkDTO>> CreateLink(int photoId, MutateLink mut)
    {
        if (photoId <= 0)
        {
            string message = $"Parameter '{nameof(photoId)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.CreateLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (mut.AccessLimit is not null && mut.AccessLimit <= 0)
        {
            string message = $"Parameter '{nameof(mut.AccessLimit)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.CreateLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (mut.ExpiresAt is not null && mut.ExpiresAt <= DateTime.Now)
        {
            string message = $"Parameter '{nameof(mut.ExpiresAt)}' cannot be an earlier date than now!";
            logging
                .Action(nameof(PublicLinkHandler.CreateLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var newLink = await linkService.CreateLink(photoId, mut);

        if (newLink.Value is null)
        {
            string message = $"Failed to create new {nameof(PublicLink)}(s) for {nameof(Photo)} #{photoId}!";
            logging
                .Action(nameof(PublicLinkHandler.CreateLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return newLink.Result!;
        }

        return (PublicLinkDTO)newLink.Value;
    }

    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public virtual Task<ActionResult<PublicLinkDTO>> UpdateLink(int linkId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLink(linkId, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public async Task<ActionResult<PublicLinkDTO>> UpdateLink(int linkId, MutateLink mut)
    {
        if (linkId <= 0)
        {
            string message = $"Parameter '{nameof(linkId)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.UpdateLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (mut.AccessLimit is not null && mut.AccessLimit <= 0)
        {
            string message = $"Parameter '{nameof(mut.AccessLimit)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.UpdateLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (mut.ExpiresAt is not null && mut.ExpiresAt <= DateTime.Now)
        {
            string message = $"Parameter '{nameof(mut.ExpiresAt)}' cannot be an earlier date than now!";
            logging
                .Action(nameof(PublicLinkHandler.UpdateLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var newLink = await linkService.UpdateLink(linkId, mut);

        if (newLink.Value is null)
        {
            string message = $"Failed to update {nameof(PublicLink)} #{linkId}!";
            logging
                .Action(nameof(PublicLinkHandler.UpdateLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return newLink.Result!;
        }

        return (PublicLinkDTO)newLink.Value;
    }

    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public virtual Task<ActionResult<PublicLinkDTO>> UpdateLinkByCode(string code, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLinkByCode(code, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public async Task<ActionResult<PublicLinkDTO>> UpdateLinkByCode(string code, MutateLink mut)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            string message = $"Parameter '{nameof(code)}' cannot be null/omitted!";
            logging
                .Action(nameof(PublicLinkHandler.UpdateLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        if (mut.AccessLimit is not null && mut.AccessLimit <= 0)
        {
            string message = $"Parameter '{nameof(mut.AccessLimit)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.UpdateLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (mut.ExpiresAt is not null && mut.ExpiresAt <= DateTime.Now)
        {
            string message = $"Parameter '{nameof(mut.ExpiresAt)}' cannot be an earlier date than now!";
            logging
                .Action(nameof(PublicLinkHandler.UpdateLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var publicLink = await linkService.UpdateLinkByCode(code, mut);

        if (publicLink.Value is null)
        {
            string message = $"Failed to update {nameof(PublicLink)} '{code}'";
            logging
                .Action(nameof(PublicLinkHandler.UpdateLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return publicLink.Result!;
        }

        return (PublicLinkDTO)publicLink.Value;
    }

    /// <summary>
    /// Delete the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public async Task<ActionResult> DeleteLink(int linkId)
    {
        if (linkId <= 0)
        {
            string message = $"Parameter '{nameof(linkId)}' has to be a non-zero positive integer!";
            logging
                .Action(nameof(PublicLinkHandler.DeleteLink))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var deleteResult = await linkService.DeleteLink(linkId);

        if (deleteResult is not OkResult &&
            deleteResult is not OkObjectResult &&
            deleteResult is not NoContentResult
        ) {
            string message = $"Failed to delete {nameof(PublicLink)} #{linkId}";
            logging
                .Action(nameof(PublicLinkHandler.DeleteLink))
                .ExternalDebug(message)
                .LogAndEnqueue();
        }

        return deleteResult;
    }
    /// <summary>
    /// Delete the <see cref="Link"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult> DeleteLinkByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            string message = $"Parameter '{nameof(code)}' cannot be null/omitted!";
            logging
                .Action(nameof(PublicLinkHandler.DeleteLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var deleteResult = await linkService.DeleteLinkByCode(code);

        if (deleteResult is not OkResult &&
            deleteResult is not OkObjectResult &&
            deleteResult is not NoContentResult
        ) {
            string message = $"Failed to delete {nameof(PublicLink)} '{code}'";
            logging
                .Action(nameof(PublicLinkHandler.DeleteLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();
        }

        return deleteResult;
    }
}
