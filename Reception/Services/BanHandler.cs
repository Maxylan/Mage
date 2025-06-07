using System.Net;
using Microsoft.AspNetCore.Mvc;
using Reception.Interfaces.DataAccess;
using Reception.Interfaces;
using Reception.Database.Models;
using Reception.Models;

namespace Reception.Services;

public class BanHandler(
    IBannedClientsService banService,
    ILoggingService<BanHandler> logging
) : IBanHandler
{
    /// <summary>
    /// Get the <see cref="BanEntry"/> with Primary Key '<paramref ref="entryId"/>'
    /// </summary>
    public async Task<ActionResult<BanEntryDTO>> GetBanEntry(int entryId)
    {
        if (entryId <= 0)
        {
            string message = $"Parameter {nameof(entryId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(BanHandler.GetBanEntry))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getEntry = await banService.GetBanEntry(entryId);

        if (getEntry.Value is null)
        {
            string message = $"Failed to find an {nameof(BanEntry)} with ID #{entryId}.";
            logging
                .Action(nameof(BanHandler.GetBanEntry))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getEntry.Result!;
        }

        return (BanEntryDTO)getEntry.Value;
    }

    /// <summary>
    /// Get all <see cref="BanEntry"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public virtual Task<ActionResult<IEnumerable<BanEntryDTO>>> GetBannedClients(Action<FilterBanEntries> opts)
    {
        FilterBanEntries filtering = new();
        opts(filtering);

        return GetBannedClients(filtering);
    }

    /// <summary>
    /// Get all <see cref="BanEntry"/>-entries matching a few optional filtering / pagination parameters.
    /// </summary>
    public async Task<ActionResult<IEnumerable<BanEntryDTO>>> GetBannedClients(FilterBanEntries filter)
    {
        if (filter.limit is not null && filter.limit <= 0)
        {
            string message = $"Parameter {nameof(filter.limit)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(BanHandler.GetBannedClients))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (filter.offset is not null && filter.offset < 0)
        {
            string message = $"Parameter {nameof(filter.offset)} has to be a positive integer!";
            logging
                .Action(nameof(BanHandler.GetBannedClients))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getEntries = await banService.GetBannedClients(filter);

        if (getEntries.Value is null)
        {
            string message = $"Failed to find any {nameof(BanEntry)} matching your given parameters.";
            logging
                .Action(nameof(BanHandler.GetBannedClients))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return getEntries.Result!;
        }

        return getEntries.Value
            .Select(e => (BanEntryDTO)e)
            .ToArray();
    }

    /// <summary>
    /// Update a <see cref="BanEntry"/> in the database.
    /// </summary>
    public async Task<ActionResult<(BanEntryDTO, bool)>> UpdateBanEntry(MutateBanEntry mut)
    {
        if (mut.Id <= 0)
        {
            string message = $"Parameter {nameof(mut.Id)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(BanHandler.UpdateBanEntry))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var updatedEntry = await banService.UpdateBanEntry(mut);

        if (updatedEntry.Value is null)
        {
            string message = $"Failed to update {nameof(BanEntry)} with ID #{mut.Id}.";
            logging
                .Action(nameof(BanHandler.UpdateBanEntry))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return updatedEntry.Result!;
        }

        return (
            (BanEntryDTO)updatedEntry.Value,
            (
                updatedEntry.Result is OkResult ||
                updatedEntry.Result is OkObjectResult
            )
        );
    }

    /// <summary>
    /// Create a <see cref="BanEntry"/> in the database.
    /// Equivalent to banning a single client (<see cref="Client"/>).
    /// </summary>
    public async Task<ActionResult<BanEntryDTO>> BanClient(MutateBanEntry mut)
    {
        var banEntry = await banService.CreateBanEntry(mut);

        if (banEntry.Value is null)
        {
            string message = $"Failed to create new {nameof(BanEntry)} for {nameof(Client)} #{mut.ClientId}";
            if (mut.Client is not null && !string.IsNullOrWhiteSpace(mut.Client.Address)) {
                message += $" ({mut.Client.Address})";
            }

            logging
                .Action(nameof(BanHandler.BanClient))
                .ExternalDebug(message + ".")
                .LogAndEnqueue();

            return banEntry.Result!;
        }

        return (BanEntryDTO)banEntry.Value;
    }

    /// <summary>
    /// Delete / Remove a <see cref="BanEntry"/> from the database.
    /// Equivalent to unbanning a single client (<see cref="Client"/>).
    /// </summary>
    public async Task<ActionResult> UnbanClient(int clientId, int accountId)
    {
        if (clientId <= 0)
        {
            string message = $"Parameter {nameof(clientId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(BanHandler.UnbanClient))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (accountId <= 0)
        {
            string message = $"Parameter {nameof(accountId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(BanHandler.UnbanClient))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getBanEntriesByClient = await banService.GetBannedClients(opts => {
            opts.clientId = clientId;
            opts.accountId = accountId;
        });

        var banEntries = getBanEntriesByClient.Value;
        if (banEntries is null || banEntries.Count() <= 0)
        {
            string message = $"No {nameof(BanEntry)} found matching {nameof(clientId)} #{clientId} and {nameof(accountId)} #{accountId}!";
            logging
                .Action(nameof(BanHandler.UnbanClient))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new NotFoundObjectResult(
                Program.IsProduction ? HttpStatusCode.NotFound.ToString() : message
            );
        }

        var deletedEntries = await Task.WhenAll(
            banEntries.Select(
                entry => banService.DeleteBanEntry(entry)
            )
        );

        if (deletedEntries.All(
            unban => unban.Result is OkResult || unban.Result is OkObjectResult
        )) {
            string message = $"Successfully deleted {deletedEntries.Count()} {nameof(BanEntry)}(s) with ID #{clientId}.";
            logging
                .Action(nameof(BanHandler.UnbanClient))
                .ExternalWarning(message)
                .LogAndEnqueue();

            return new NoContentResult();
        }
        else if (deletedEntries.Any(
            unban => unban.Result is OkResult || unban.Result is OkObjectResult
        )) {
            string message = $"Partial success deleting existing {nameof(BanEntry)}(s) with ID #{clientId}.";
            logging
                .Action(nameof(BanHandler.UnbanClient))
                .ExternalWarning(message)
                .LogAndEnqueue();

            return new ObjectResult(message) {
                StatusCode = StatusCodes.Status206PartialContent
            };
        }

        string failMessage = $"Failed to delete {nameof(BanEntry)}(s) matching {nameof(clientId)} #{clientId} and {nameof(accountId)} #{accountId}.";
        logging
            .Action(nameof(BanHandler.UnbanClient))
            .ExternalDebug(failMessage)
            .LogAndEnqueue();

        return deletedEntries.First().Result!;
    }
}
