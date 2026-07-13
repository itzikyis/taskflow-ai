using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Calendar.Queries.GetCalendarFeed;

namespace TaskFlow.API.Controllers;

/// <summary>Calendar (iCalendar) integration endpoints.</summary>
[ApiController]
[Route("api/calendar")]
public sealed class CalendarController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns an iCalendar feed of the user's due-dated tasks. The user id in the
    /// URL is the secret feed token — calendar apps poll this URL directly, so it
    /// is intentionally anonymous (no Authorization header).
    /// </summary>
    [HttpGet("feed/{userId:guid}.ics")]
    [Produces("text/calendar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Feed(Guid userId, CancellationToken ct)
    {
        var ical = await mediator.Send(new GetCalendarFeedQuery(userId), ct);
        return File(System.Text.Encoding.UTF8.GetBytes(ical), "text/calendar; charset=utf-8", "taskflow.ics");
    }

    /// <summary>Returns the current user's personal calendar feed URL.</summary>
    [HttpGet("feed-url")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult FeedUrl()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new { url = $"{baseUrl}/api/calendar/feed/{userId}.ics" });
    }
}
