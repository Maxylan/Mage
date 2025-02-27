using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Authentication;
using Reception.Interfaces;
using Reception.Models;
using Reception.Models.Entities;

namespace Reception.Services;

public class LinkService(
    MageDbContext db,
    ILoggingService logging,
    IHttpContextAccessor contextAccessor,
    IPhotoService photos
) : ILinkService
{
    /// <summary>
    /// Get the <see cref="Uri"/> of a <see cref="Link"/>
    /// </summary>
    public static Uri GenerateLinkUri(string code, Dimension? dimension = null)
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
    /// Get the <see cref="Uri"/> of a <see cref="Link"/>
    /// </summary>
    public Uri LinkUri(string code, Dimension? dimension = null) =>
        GenerateLinkUri(code, dimension);

    /// <summary>
    /// Get all <see cref="Link"/> entries.
    /// <br/><paramref name="includeInactive"/><c> = false</c> can be used to only view active links.
    /// </summary>
    /// <param name="includeInactive">
    /// Passing <c>false</c> would be equivalent to filtering by active links.
    /// </param>
    public async Task<ActionResult<IEnumerable<Link>>> GetLinks(bool includeInactive, int limit = 99, int offset = 0)
    {
        if (limit <= 0) {
            throw new ArgumentOutOfRangeException(nameof(limit), $"Parameter {nameof(limit)} has to be a non-zero positive integer!");
        }
        if (offset < 0) {
            throw new ArgumentOutOfRangeException(nameof(offset), $"Parameter {nameof(offset)} has to be a positive integer!");
        }

        if (!includeInactive)
        {
            return await db.Links
                .Where(link => link.ExpiresAt > DateTime.UtcNow)
                .Where(link => link.AccessLimit == null || link.AccessLimit <= 0 || link.Accessed < link.AccessLimit)
                .OrderBy(link => link.ExpiresAt)
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync();
        }

        return await db.Links
            .OrderByDescending(link => link.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToArrayAsync();
    }

    /// <summary>
    /// Get the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public async Task<ActionResult<Link>> GetLink(int linkId)
    {
        if (linkId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(linkId), $"Parameter {nameof(linkId)} has to be a non-zero positive integer!");
        }

        Link? link = await db.Links.FindAsync(linkId);

        if (link is null)
        {
            await logging
                .Action(nameof(GetLink))
                .InternalDebug($"Link with ID #{linkId} could not be found.")
                .SaveAsync();

            return new NotFoundObjectResult($"{nameof(Link)} with ID #{linkId} not found!");
        }

        return link;
    }
    /// <summary>
    /// Get the <see cref="Link"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult<Link>> GetLinkByCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, $"Parameter {nameof(code)} cannot be null/empty!");

        if (!code.IsNormalized())
        {
            code = code
                .Normalize()
                .Trim();
        }

        Link? link = await db.Links.FirstOrDefaultAsync(link => link.Code == code);

        if (link is null)
        {
            await logging
                .Action(nameof(GetLinkByCode))
                .InternalDebug($"Link with code '{code}' could not be found.")
                .SaveAsync();

            return new NotFoundObjectResult($"{nameof(Link)} with unique code '{code}' could not be found!");
        }

        return link;
    }

    /// <summary>
    /// Create a <see cref="Link"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public virtual Task<ActionResult<Link>> CreateLink(int photoId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return CreateLink(photoId, mutationOptions);
    }
    /// <summary>
    /// Create a <see cref="Link"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photoId"/>'.
    /// </summary>
    public async Task<ActionResult<Link>> CreateLink(int photoId, MutateLink mut)
    {
        if (photoId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(photoId), $"Parameter {nameof(photoId)} has to be a non-zero positive integer!");
        }

        var getPhoto = await photos.GetPhotoEntity(photoId);
        var photo = getPhoto.Value;

        if (photo is null) {
            return getPhoto.Result!;
        }

        Account? user = null;
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(CreateLink)} Failed: No {nameof(HttpContext)} found.";
            await logging
                .Action(nameof(CreateLink))
                .InternalError(message)
                .SaveAsync();

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
                await logging
                    .Action(nameof(CreateLink))
                    .ExternalError($"Cought an '{ex.GetType().FullName}' invoking {nameof(MageAuthentication.GetAccount)}!", opts => {
                        opts.Exception = ex;
                        opts.SetUser(user);
                    })
                    .SaveAsync();
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

        Link link = new()
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
            photo.Links.Add(link);
            db.Update(photo);

            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)}. ";
            await logging
                .Action(nameof(CreateLink))
                .InternalError(message + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    opts.SetUser(user);
                })
                .SaveAsync();

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
            await logging
                .Action(nameof(CreateLink))
                .InternalError(message + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    opts.SetUser(user);
                })
                .SaveAsync();

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
    /// Increment the <see cref="Link.Accessed"/> property of a <see cref="Link"/>.
    /// </summary>
    public async Task<Link> IncrementLinkAccessed(Link link, bool reload = true)
    {
        if (reload) {
            await db.Links.Entry(link).ReloadAsync();
        }

        link.Accessed++;
        db.Links.Update(link);

        return link;
    }

    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    public virtual Task<ActionResult<Link>> UpdateLink(int linkId, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLink(linkId, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    public async Task<ActionResult<Link>> UpdateLink(int linkId, MutateLink mut)
    {
        throw new NotImplementedException();

        try
        {
            // db.Update(link);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException updateException)
        {
            string message = $"Cought a {nameof(DbUpdateException)}. ";
            await logging
                .Action(nameof(UpdateLink))
                .InternalError(message + updateException.Message, opts =>
                {
                    opts.Exception = updateException;
                    // opts.SetUser(user);
                })
                .SaveAsync();

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
            await logging
                .Action(nameof(UpdateLink))
                .InternalError(message + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    // opts.SetUser(user);
                })
                .SaveAsync();

            return new ObjectResult(message + (
                Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : ex.Message
            ))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    public virtual Task<ActionResult<Link>> UpdateLinkByCode(string code, Action<MutateLink> opts)
    {
        MutateLink mutationOptions = new();
        opts(mutationOptions);

        return UpdateLinkByCode(code, mutationOptions);
    }
    /// <summary>
    /// Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
    /// </summary>
    public async Task<ActionResult<Link>> UpdateLinkByCode(string code, MutateLink mut)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
    /// </summary>
    public async Task<ActionResult> DeleteLink(int linkId)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Delete the <see cref="Link"/> with Unique '<paramref ref="code"/>'
    /// </summary>
    public async Task<ActionResult> DeleteLinkByCode(string code)
    {
        throw new NotImplementedException();
    }
}
