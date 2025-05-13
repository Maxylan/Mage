using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Middleware.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Database;
using Reception.Models;

namespace Reception.Services.DataAccess;

public class PublicLinkService(
    MageDb db,
    ILoggingService<PublicLinkService> logging,
    IHttpContextAccessor contextAccessor,
    IPhotoService photos
) : IPublicLinkService
{
    /// <summary>
    /// Get the <see cref="Uri"/> of a <see cref="PublicLink"/>
    /// </summary>
    public Uri GetUri(string code, Dimension? dimension = null)
    {
        string dim = dimension switch
        {
            Dimension.SOURCE => "source",
            Dimension.MEDIUM => "medium",
            Dimension.THUMBNAIL => "thumbnail",
            _ => "source"
        };

        return new($"{(Program.ApiUrl ?? "localhost")}/reception/links/view/{dim}/{code}");
    }

    /// <summary>
    /// Get the <see cref="Uri"/> of a <paramref name="link"/> (<see cref="PublicLink"/>)
    /// </summary>
    public Uri GetUri(PublicLink link, Dimension? dimension = null) =>
        this.GetUri(link.Code, dimension);

    /// <summary>
    /// Get the <see cref="PublicLink"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public async Task<ActionResult<PublicLink>> GetLink(int linkId)
    {
        if (linkId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(linkId), $"Parameter {nameof(linkId)} has to be a non-zero positive integer!");
        }

        PublicLink? link = await db.Links.FindAsync(linkId);

        if (link is null)
        {
            logging
                .Action(nameof(GetLink))
                .InternalDebug($"Link with ID #{linkId} could not be found.")
                .LogAndEnqueue();

            return new NotFoundObjectResult($"{nameof(PublicLink)} with ID #{linkId} not found!");
        }

        return link;
    }
    /// <summary>
    /// Get the <see cref="PublicLink"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult<PublicLink>> GetLinkByCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, $"Parameter {nameof(code)} cannot be null/empty!");

        if (!code.IsNormalized())
        {
            code = code
                .Normalize()
                .Trim();
        }

        PublicLink? link = await db.Links.FirstOrDefaultAsync(link => link.Code == code);

        if (link is null)
        {
            logging
                .Action(nameof(GetLinkByCode))
                .InternalDebug($"Link with code '{code}' could not be found.")
                .LogAndEnqueue();

            return new NotFoundObjectResult($"{nameof(PublicLink)} with unique code '{code}' could not be found!");
        }

        return link;
    }

    /// <summary>
    /// Get all <see cref="PublicLink"/> entries.
    /// </summary>
    public async Task<ActionResult<IEnumerable<PublicLink>>> GetLinks(int limit = 99, int offset = 0)
    {
        if (limit <= 0) {
            throw new ArgumentOutOfRangeException(nameof(limit), $"Parameter {nameof(limit)} has to be a non-zero positive integer!");
        }
        if (offset < 0) {
            throw new ArgumentOutOfRangeException(nameof(offset), $"Parameter {nameof(offset)} has to be a positive integer!");
        }

        return await db.Links
            .OrderByDescending(link => link.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToArrayAsync();
    }

    /// <summary>
    /// Create a <see cref="PublicLink"/> to the <see cref="Photo"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public virtual Task<ActionResult<PublicLink>> CreateLink(int photoId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return CreateLink(photoId, mutationOptions);
    }
    /// <summary>
    /// Create a <see cref="PublicLink"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public async Task<ActionResult<PublicLink>> CreateLink(int photoId, MutateLink mut)
    {
        if (photoId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(photoId), $"Parameter {nameof(photoId)} has to be a non-zero positive integer!");
        }

        var getPhoto = await photos.GetPhoto(photoId);
        var photo = getPhoto.Value;

        if (photo is null) {
            return getPhoto.Result!;
        }

        Account? user = null;
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(CreateLink)} Failed: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(CreateLink))
                .InternalError(message)
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : message) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        if (MageAuthentication.IsAuthenticated(contextAccessor))
        {
            try
            {
                user = MageAuthentication.GetAccount(contextAccessor);
            }
            catch (Exception ex)
            {
                logging
                    .Action(nameof(CreateLink))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => {
                        opts.Exception = ex;
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();
            }
        }

        foreach(var navigation in db.Entry(photo).Navigations) {
            if (!navigation.IsLoaded) {
                await navigation.LoadAsync();
            }
        }

        if (mut.ExpiresAt is null || mut.ExpiresAt.Value < DateTime.UtcNow)
        {   // Default omitted ExpiresAt to 3 months..
            mut.ExpiresAt = DateTime.UtcNow.AddMonths(3);
        }

        if (mut.AccessLimit == 0)
        {   // Default omitted AccessLimit to null..
            mut.AccessLimit = null;
        }

        PublicLink link = new()
        {
            PhotoId = photoId,
            Code = Guid.NewGuid().ToString("N"),
            CreatedBy = user?.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = mut.ExpiresAt.Value,
            AccessLimit = mut.AccessLimit,
            Accessed = 0
        };

        try
        {
            photo.PublicLinks.Add(link);
            db.Update(photo);

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)}. ";
            logging
                .Action(nameof(CreateLink))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}'. ";
            logging
                .Action(nameof(CreateLink))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return link;
    }

    /// <summary>
    /// Increment the <see cref="Link.Accessed"/> property of a <see cref="PublicLink"/>.
    /// </summary>
    public async Task<PublicLink> LinkAccessed(PublicLink link)
    {
        await db.Links
            .Entry(link)
            .ReloadAsync();

        link.Accessed++;
        db.Links.Update(link);

        return link;
    }

    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public virtual Task<ActionResult<PublicLink>> UpdateLink(int linkId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLink(linkId, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public async Task<ActionResult<PublicLink>> UpdateLink(int linkId, MutateLink mut)
    {
        ArgumentNullException.ThrowIfNull(mut);
        ArgumentNullException.ThrowIfNull(linkId);

        if (linkId <= 0)
        {
            string message = $"Parameter '{nameof(linkId)}' has to be a non-zero positive integer! (Link ID)";
            logging
                .Action(nameof(UpdateLink))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        PublicLink? existingLink = await db.Links.FindAsync(linkId);

        if (existingLink is null)
        {
            string message = $"{nameof(PublicLink)} with ID #{linkId} could not be found!";
            logging
                .Action(nameof(UpdateLink))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        if (mut.AccessLimit is not null)
        {
            if (mut.AccessLimit <= 0) {
                mut.AccessLimit = 0;
            }
        }

        if (mut.ExpiresAt is null)
        {
            mut.ExpiresAt = existingLink.ExpiresAt;
        }
        else if (mut.ExpiresAt < DateTime.UtcNow)
        {
            string message = $"Parameter '{nameof(mut.ExpiresAt)}' cannot be a past date!";
            logging
                .Action(nameof(UpdateLink))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        existingLink.AccessLimit = mut.AccessLimit;
        existingLink.ExpiresAt = mut.ExpiresAt.Value;

        try
        {
            db.Update(existingLink);
            logging
                .Action(nameof(UpdateLink))
                .InternalInformation($"Expiry settings on link '{existingLink.Code}' (#{existingLink.Id}) updated to; Expires: {existingLink.ExpiresAt}, AccessLimit: {(existingLink.AccessLimit?.ToString() ?? "null")}.")
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update {nameof(PublicLink)} '{existingLink.Code}' (#{existingLink.Id}).";
            logging
                .Action(nameof(UpdateLink))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update {nameof(PublicLink)} '{existingLink.Code}' (#{existingLink.Id}).";
            logging
                .Action(nameof(UpdateLink))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return existingLink;
    }

    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public virtual Task<ActionResult<PublicLink>> UpdateLinkByCode(string code, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLinkByCode(code, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="PublicLink"/> to a <see cref="Photo"/>.
    /// </summary>
    public async Task<ActionResult<PublicLink>> UpdateLinkByCode(string code, MutateLink mut)
    {
        ArgumentNullException.ThrowIfNull(mut);

        if (string.IsNullOrWhiteSpace(code))
        {
            string message = $"Parameter '{nameof(code)}' cannot be null/empty!";
            logging
                .Action(nameof(UpdateLinkByCode))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }
        if (code.Length != 32)
        {
            string message = $"Parameter '{nameof(code)}' invalid format!";
            logging
                .Action(nameof(UpdateLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        PublicLink? existingLink = await db.Links
            .FirstOrDefaultAsync(link => link.Code == code);

        if (existingLink is null)
        {
            string message = $"{nameof(PublicLink)} with unique code '{code}' could not be found!";
            logging
                .Action(nameof(UpdateLinkByCode))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        if (mut.AccessLimit is not null)
        {
            if (mut.AccessLimit <= 0) {
                mut.AccessLimit = 0;
            }
        }

        if (mut.ExpiresAt is null)
        {
            mut.ExpiresAt = existingLink.ExpiresAt;
        }
        else if (mut.ExpiresAt < DateTime.UtcNow)
        {
            string message = $"Parameter '{nameof(mut.ExpiresAt)}' cannot be a past date!";
            logging
                .Action(nameof(UpdateLinkByCode))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        existingLink.AccessLimit = mut.AccessLimit;
        existingLink.ExpiresAt = mut.ExpiresAt.Value;

        try
        {
            db.Update(existingLink);
            logging
                .Action(nameof(UpdateLinkByCode))
                .InternalInformation($"Expiry settings on link '{existingLink.Code}' (#{existingLink.Id}) updated to; Expires: {existingLink.ExpiresAt}, AccessLimit: {(existingLink.AccessLimit?.ToString() ?? "null")}.")
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to update {nameof(PublicLink)} '{existingLink.Code}' (#{existingLink.Id}).";
            logging
                .Action(nameof(UpdateLinkByCode))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to update {nameof(PublicLink)} '{existingLink.Code}' (#{existingLink.Id}).";
            logging
                .Action(nameof(UpdateLinkByCode))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return existingLink;
    }

    /// <summary>
    /// Delete the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public async Task<ActionResult> DeleteLink(int linkId)
    {
        ArgumentNullException.ThrowIfNull(linkId);

        if (linkId <= 0)
        {
            string message = $"Parameter '{nameof(linkId)}' has to be a non-zero positive integer! (Link ID)";
            logging
                .Action(nameof(DeleteLink))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        PublicLink? existingLink = await db.Links.FindAsync(linkId);

        if (existingLink is null)
        {
            string message = $"{nameof(PublicLink)} with ID #{linkId} could not be found!";
            logging
                .Action(nameof(DeleteLink))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        try
        {
            db.Update(existingLink);
            logging
                .Action(nameof(DeleteLink))
                .InternalInformation($"Link '{existingLink.Code}' (#{existingLink.Id}) was just removed.")
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(PublicLink)} '{existingLink.Code}' (#{existingLink.Id}).";
            logging
                .Action(nameof(DeleteLink))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(PublicLink)} '{existingLink.Code}' (#{existingLink.Id}).";
            logging
                .Action(nameof(DeleteLink))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return new NoContentResult();
    }
    /// <summary>
    /// Delete the <see cref="Link"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult> DeleteLinkByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            string message = $"Parameter '{nameof(code)}' cannot be null/empty!";
            logging
                .Action(nameof(DeleteLinkByCode))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }
        if (code.Length != 32)
        {
            string message = $"Parameter '{nameof(code)}' invalid format!";
            logging
                .Action(nameof(DeleteLinkByCode))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(message);
        }

        PublicLink? existingLink = await db.Links
            .FirstOrDefaultAsync(link => link.Code == code);

        if (existingLink is null)
        {
            string message = $"{nameof(PublicLink)} with unique code '{code}' could not be found!";
            logging
                .Action(nameof(DeleteLinkByCode))
                .InternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(message);
        }

        try
        {
            db.Remove(existingLink);
            logging
                .Action(nameof(DeleteLinkByCode))
                .InternalInformation($"Link '{existingLink.Code}' (#{existingLink.Id}) was just removed.")
                .LogAndEnqueue();

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)} attempting to delete {nameof(PublicLink)} '{existingLink.Code}' (#{existingLink.Id}).";
            logging
                .Action(nameof(DeleteLinkByCode))
                .InternalError(message + " " + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : updateException.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            string message = $"Cought an unkown exception of type '{ex.GetType().FullName}' while attempting to delete {nameof(PublicLink)} '{existingLink.Code}' (#{existingLink.Id}).";
            logging
                .Action(nameof(DeleteLinkByCode))
                .InternalError(message + " " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                })
                .LogAndEnqueue();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        return new NoContentResult();
    }
}
