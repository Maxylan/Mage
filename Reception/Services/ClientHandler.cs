using System.Net;
using Microsoft.AspNetCore.Mvc;
using Reception.Middleware.Authentication;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Services;

public class ClientHandler(
    ILoggingService<ClientHandler> logging,
    IClientService clientService
) : IClientHandler
{
    /// <summary>
    /// Get the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>'
    /// </summary>
    public async Task<ActionResult<DisplayClient>> GetClient(int clientId)
    {
        if (clientId <= 0)
        {
            string message = $"Parameter {nameof(clientId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(GetClient))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getClient = await clientService.GetClient(clientId);
        Client? client = getClient.Value;

        if (client is null)
        {
            string message = $"Failed to get a {nameof(Client)} matching the given ID #{clientId}.";
            logging
                .Action(nameof(GetClient))
                .LogDebug(message)
                .LogAndEnqueue();

            return getClient.Result!;
        }

        return new DisplayClient(client);
    }

    /// <summary>
    /// Get the <see cref="Client"/> with Fingerprint '<paramref ref="address"/>' & '<paramref ref="userAgent"/>'.
    /// </summary>
    public async Task<ActionResult<DisplayClient>> GetClientByFingerprint(string address, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            string message = $"Parameter {nameof(address)} cannot be null/omitted!";
            logging
                .Action(nameof(GetClientByFingerprint))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getClient = await clientService.GetClientByFingerprint(address, userAgent);
        Client? client = getClient.Value;

        if (client is null)
        {
            string message = $"Failed to get a {nameof(Client)} matching the given address '{address}'.";
            logging
                .Action(nameof(GetClientByFingerprint))
                .LogDebug(message)
                .LogAndEnqueue();

            return getClient.Result!;
        }

        return new DisplayClient(client);
    }

    /// <summary>
    /// Check if the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>' (int) is banned.
    /// </summary>
    public async Task<ActionResult<BanEntryDTO?>> IsBanned(int clientId)
    {
        if (clientId <= 0)
        {
            string message = $"Parameter {nameof(clientId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(GetClient))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var isClientBanned = await clientService.IsBanned(clientId);

        if (isClientBanned.Value is null)
        {
            return isClientBanned.Result!;
        }

        return (BanEntryDTO)isClientBanned.Value;
    }

    /// <summary>
    /// Check if the <see cref="Client"/> '<paramref ref="client"/>' is banned.
    /// </summary>
    public async Task<ActionResult<BanEntryDTO?>> IsBanned(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);

        var isClientBanned = await clientService.IsBanned(client);

        if (isClientBanned.Value is null)
        {
            return isClientBanned.Result!;
        }

        return (BanEntryDTO)isClientBanned.Value;
    }

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<ClientDTO>>> GetClients(Action<FilterClients> opts)
    {
        FilterClients filtering = new();
        opts(filtering);

        return GetClients(filtering);
    }

    /// <summary>
    /// Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<ClientDTO>>> GetClients(FilterClients filter)
    {
        if (filter.limit is not null && filter.limit <= 0)
        {
            string message = $"Parameter {nameof(filter.limit)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(GetClients))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (filter.offset is not null && filter.offset < 0)
        {
            string message = $"Parameter {nameof(filter.offset)} has to be a positive integer!";
            logging
                .Action(nameof(GetClients))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getClients = await clientService.GetClients(filter);

        if (getClients.Value is null)
        {
            return getClients.Result!;
        }

        return getClients.Value
            .Select(client => (ClientDTO)client)
            .ToArray();
    }

    /// <summary>
    /// Delete / Remove an <see cref="Client"/> from the database.
    /// </summary>
    public async Task<ActionResult> DeleteClient(int clientId)
    {
        if (clientId <= 0)
        {
            string message = $"Parameter {nameof(clientId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(DeleteClient))
                .LogDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var deleteClient = await clientService.DeleteClient(clientId);

        if (deleteClient is not OkResult &&
            deleteClient is not OkObjectResult &&
            deleteClient is not NoContentResult
        ) {
            string message = $"Failed to delete {nameof(Client)} matching the given ID #{clientId}.";
            logging
                .Action(nameof(DeleteClient))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return deleteClient;
        }

        return deleteClient;
    }
}
