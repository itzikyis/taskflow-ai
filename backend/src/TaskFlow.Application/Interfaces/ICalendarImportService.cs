namespace TaskFlow.Application.Interfaces;

/// <summary>Service that fetches and parses an external iCalendar feed.</summary>
public interface ICalendarImportService
{
    /// <summary>
    /// Fetches the iCalendar document at <paramref name="icalUrl"/> and returns the parsed events
    /// as (Title, DueDate) pairs. Events without a parseable date are silently skipped.
    /// </summary>
    /// <param name="icalUrl">The absolute URL of the external iCal feed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of (Title, DueDate) value tuples extracted from the feed.</returns>
    Task<List<(string Title, DateTime DueDate)>> ImportEventsAsync(string icalUrl, CancellationToken ct = default);
}
