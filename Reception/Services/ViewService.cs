using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reception.Authentication;
using Reception.Models.Entities;
using Reception.Interfaces;
using Reception.Utilities;

namespace Reception.Services;

public class ViewService(
    ILoggingService<ViewService> logging,
    IHttpContextAccessor contextAccessor,
    IPhotoService photoService,
    ILinkService linkService,
    IBlobService blobs
) : IViewService
{
    /// <summary>
    /// View the Source <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// Disguises a lot of responses outside of Development, since this is deals with publically available URL's, I don't want to encourage pen-testing or scraping.
    /// <para>
    ///     A valid <see cref="Link"/> that's expired will return an HTTP 410 'Gone' response status.
    /// </para>
    /// </remarks>
    public virtual Task<ActionResult> ViewSource(Guid? code) =>
        View(Dimension.SOURCE, code);

    /// <summary>
    /// View the Medium <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// Disguises a lot of responses outside of Development, since this is deals with publically available URL's, I don't want to encourage pen-testing or scraping.
    /// <para>
    ///     A valid <see cref="Link"/> that's expired will return an HTTP 410 'Gone' response status.
    /// </para>
    /// </remarks>
    public virtual Task<ActionResult> ViewMedium(Guid? code) =>
        View(Dimension.MEDIUM, code);

    /// <summary>
    /// View the Medium <see cref="PhotoEntity"/> (blob) associated with the <see cref="Link"/> with Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// Disguises a lot of responses outside of Development, since this is deals with publically available URL's, I don't want to encourage pen-testing or scraping.
    /// <para>
    ///     A valid <see cref="Link"/> that's expired will return an HTTP 410 'Gone' response status.
    /// </para>
    /// </remarks>
    public virtual Task<ActionResult> ViewThumbnail(Guid? code) =>
        View(Dimension.THUMBNAIL, code);

    /// <summary>
    /// View the <see cref="PhotoEntity"/> (<paramref name="dimension"/>, blob) associated with the <see cref="Link"/> with
    /// Unique Code (GUID) '<paramref ref="code"/>'
    /// </summary>
    /// <remarks>
    /// <paramref name="dimension"/> Controls what image size is returned.
    /// <para>
    ///     Disguises a lot of responses outside of Development, since this is deals with publically available URL's,
    ///     I don't want to encourage pen-testing or scraping.
    /// </para>
    /// <para>
    ///     A valid <see cref="Link"/> that's expired will return an HTTP 410 'Gone' response status.
    /// </para>
    /// </remarks>
    public async Task<ActionResult> View(Dimension dimension, Guid? code)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            string message = $"{nameof(ViewService.View)} Failed to generate {dimension.ToString()} view: No {nameof(HttpContext)} found.";
            logging
                .Action(nameof(ViewService.View))
                .InternalError(message)
                .LogAndEnqueue();

            return new ObjectResult(Program.IsProduction ? HttpStatusCode.InternalServerError.ToString() : message) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        Link? link = null;
        Account? user = null; // Authentication is *not* a requirement here, but it we can *try* to enhance logging..
        bool isAuthenticated = MageAuthentication.TryGetAccount(contextAccessor, out user);
        string? userAddress = MageAuthentication.GetRemoteAddress(httpContext);
        string? userAgent = httpContext.Request.Headers.UserAgent.ToString();

        try {
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                userAgent = userAgent
                    .Normalize()
                    .Subsmart(0, 1023)
                    .Replace("\\", "\\\\")
                    .Replace("&&", "and")
                    .Trim();
                // Could also use '/[^\x20-\x7E]/g' if we wanted to go next-level
                // ..but I feel like Regex opens its own pandora's box of attack vectors.
            }

            string? linkCode = code?.ToString("N")?.Normalize()?.Trim();
            if (string.IsNullOrWhiteSpace(linkCode) || linkCode.Length != 32)
            {
                logging
                    .Action(nameof(ViewService.View))
                    .ExternalSuspicious($"Bad attempt to view a source ('{(linkCode ?? "null")}')", opts => {
                        opts.RequestAddress = userAddress;
                        opts.RequestUserAgent = userAgent;
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                // Do not reveal any more info than necessary in when responding to potentially malicious requests / pen-tests..
                return new NotFoundResult();
            }

            var getLink = await linkService.GetLinkByCode(linkCode);
            link = getLink.Value;

            if (link is null)
            {
                return Program.IsProduction
                    ? new NotFoundResult()
                    : getLink.Result!;
            }

            bool expired = false;
            if (link.ExpiresAt < DateTime.UtcNow)
            {
                logging
                    .Action(nameof(ViewService.View))
                    .ExternalInformation($"Link '{linkCode}' has expired. {nameof(Link.ExpiresAt)} date has passed.", opts => {
                        opts.RequestAddress = userAddress;
                        opts.RequestUserAgent = userAgent;
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                expired = true;
            }
            else if (link.AccessLimit is not null && link.Accessed >= link.AccessLimit)
            {
                logging
                    .Action(nameof(ViewService.View))
                    .ExternalInformation($"Link '{linkCode}' has expired. {nameof(Link.AccessLimit)} reached.", opts => {
                        opts.RequestAddress = userAddress;
                        opts.RequestUserAgent = userAgent;
                        opts.SetUser(user);
                    })
                    .LogAndEnqueue();

                expired = true;
            }

            if (expired)
            {
                return new ObjectResult($"Link '{code}' has expired.")
                {
                    StatusCode = StatusCodes.Status410Gone
                };
            }
        }
        catch (Exception ex)
        {
            string uriForDebugging = (
                link is null ? "null" : LinkService.GenerateLinkUri(link.Code, dimension).ToString()
            );

            logging
                .Action(nameof(ViewService.View))
                .ExternalCritical($"A way to induce/throw an {ex.GetType().Name} in {nameof(ViewService.View)} was found! ('{uriForDebugging}') " + ex.Message, opts =>
                {
                    opts.Exception = ex;
                    opts.RequestAddress = userAddress;
                    opts.RequestUserAgent = userAgent;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            // Do not reveal any more info than necessary in when responding to potentially malicious requests / pen-tests..
            return new NotFoundResult();
        }

        try {
            // Log the visit.
            link = await linkService.IncrementLinkAccessed(link);

            if (link.Photo is null)
            {
                var getPhoto = await photoService.GetPhotoEntity(link.PhotoId);
                if (getPhoto.Value is null) {
                    return new NotFoundResult();
                }

                link.Photo = getPhoto.Value;
            }

            string uri = LinkService.GenerateLinkUri(link.Code, dimension).ToString();
            string visitMessage = $"Link '{uri}' visited (Total visits: {link.Accessed})";

            if (user is not null) {
                visitMessage += $" ..by {user.Username} (#{user.Id})";
            }

            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                if (userAgent.Length >= 48) {
                    visitMessage += $". UserAgent '{userAgent.Substring(0, 46)}..'";

                }
                else {
                    visitMessage += $". UserAgent '{userAgent}'";
                }
            }

            if (!string.IsNullOrWhiteSpace(userAddress))
            {
                if (userAddress.Length >= 48) {
                    visitMessage += $". Address: {userAddress.Substring(0, 46)}..'";

                }
                else {
                    visitMessage += $". Address: '{userAddress}'";
                }
            }

            logging
                .Action(nameof(ViewService.View))
                .ExternalInformation(visitMessage + ".", opts =>
                {
                    opts.RequestAddress = userAddress;
                    opts.RequestUserAgent = userAgent;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            // Get & Return the requested `Photo`..
            return await blobs.GetBlobAsync(dimension, link.Photo);
        }
        catch (DbUpdateException ex)
        {
            logging
                .Action(nameof(ViewService.View))
                .InternalError($"Cought an {ex.GetType().Name} in {nameof(ViewService.View)} while querying the database! {ex.Message}", opts =>
                {
                    opts.Exception = ex;
                    opts.RequestAddress = userAddress;
                    opts.RequestUserAgent = userAgent;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult($"Internal Server Error." + (Program.IsDevelopment ? $" ({ex.GetType().Name}) {ex.Message}" : "")) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (Exception ex)
        {
            logging
                .Action(nameof(ViewService.View))
                .InternalError($"Cought an unknown exception of type {ex.GetType().Name} in {nameof(ViewService.View)}! {ex.Message}", opts =>
                {
                    opts.Exception = ex;
                    opts.RequestAddress = userAddress;
                    opts.RequestUserAgent = userAgent;
                    opts.SetUser(user);
                })
                .LogAndEnqueue();

            return new ObjectResult($"Internal Server Error." + (Program.IsDevelopment ? $" ({ex.GetType().Name}) {ex.Message}" : "")) {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
