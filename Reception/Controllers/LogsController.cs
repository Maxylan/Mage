using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Reception.Interfaces.DataAccess;
using Reception.Models;
using Reception.Database.Models;
using System.ComponentModel.DataAnnotations;
using Reception.Database;

namespace Reception.Controllers;

[Authorize]
[ApiController]
[Route("logs")]
[Produces("application/json")]
public class LogsController(IEventLogService handler) : ControllerBase
{
    /// <summary>
    /// Get a single <see cref="LogEntry"/>
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LogEntry>> Get(int id) =>
        await handler.GetEventLog(id);

    /// <summary>
    /// Get all <see cref="LogEntry"/>-instances, optionally filtered and/or paginated by a wide range of query parameters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IStatusCodeActionResult>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<LogEntry>>> GetAll(
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null,
        [FromQuery] Source? source = null,
        [FromQuery] Severity? severity = null,
        [FromQuery] Method? method = null,
        [FromQuery] string? action = null
    ) => await handler.GetEventLogs(limit, offset, source, severity, method, action);

    /// <summary>
    /// Delete a single <see cref="LogEntry"/>, returns `204` if successfull and `304` if no changes were made.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IStatusCodeActionResult> Delete(int id)
    {
        var getLogEntry = await handler.GetEventLog(id);
        var entry = getLogEntry.Value;

        if (entry is null)
        {
            return NotFound();
        }

        return await handler.DeleteEvents(entry) <= 0
            ? StatusCode(StatusCodes.Status304NotModified)
            : NoContent();
    }

    /// <summary>
    /// Delete all <see cref="LogEntry"/>-instances matching optionally filtered and paginated by a wide range of query parameters.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IStatusCodeActionResult> DeleteAll(
        [Required][FromQuery] int limit,
        [Required][FromQuery] int offset,
        [FromQuery] Source? source = null,
        [FromQuery] Severity? severity = null,
        [FromQuery] Method? method = null,
        [FromQuery] string? action = null
    )
    {
        if (limit <= 0)
        {
            return BadRequest($"Parameter {nameof(limit)} cannot be null/omitted, and has to be a positive integer greater-than `0`.");
        }
        if (offset < 0)
        {
            return BadRequest($"Parameter {nameof(offset)} cannot be null/omitted, and has to either be `0`, or any positive integer greater-than `0`.");
        }

        var getMatchingEntries = await handler.GetEventLogs(limit, offset, source, severity, method, action);
        var entries = getMatchingEntries.Value;

        if (entries is null)
        {
            return NotFound();
        }
        if (!entries.Any())
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        return await handler.DeleteEvents(entries.ToArray()) <= 0
            ? StatusCode(StatusCodes.Status304NotModified)
            : NoContent();
    }
}
