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
    public async Task<ActionResult<IEnumerable<BanEntryDTO>>> GetBannedClients(
        string? address,
        string? userAgent,
        int? userId,
        string? username,
        int? limit = 99,
        int? offset = 0
    ) {
        if (limit is not null && limit <= 0)
        {
            string message = $"Parameter {nameof(limit)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(BanHandler.GetBannedClients))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }
        if (offset is not null && offset < 0)
        {
            string message = $"Parameter {nameof(offset)} has to be a positive integer!";
            logging
                .Action(nameof(BanHandler.GetBannedClients))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var getEntries = await banService.GetBannedClients(
            address,
            userAgent,
            userId,
            username,
            limit,
            offset
        );

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
        if (mut.Id <= 0)
        {
            string message = $"Parameter {nameof(mut.Id)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(BanHandler.BanClient))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

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
    public async Task<ActionResult> UnbanClient(int entryId)
    {
        if (entryId <= 0)
        {
            string message = $"Parameter {nameof(entryId)} has to be a non-zero positive integer!";
            logging
                .Action(nameof(BanHandler.UnbanClient))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return new BadRequestObjectResult(
                Program.IsProduction ? HttpStatusCode.BadRequest.ToString() : message
            );
        }

        var banEntry = await banService.DeleteBanEntry(entryId);

        if (banEntry.Value <= 0 ||
            banEntry.Result is not OkResult ||
            banEntry.Result is not OkObjectResult
        ) {
            string message = $"Failed to delete existing {nameof(BanEntry)} with ID #{entryId}.";
            logging
                .Action(nameof(BanHandler.UnbanClient))
                .ExternalDebug(message)
                .LogAndEnqueue();

            return banEntry.Result!;
        }

        return new NoContentResult();
    }
}
