using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// Fetches and parses an external iCalendar feed using <see cref="HttpClient"/>.
/// Events are parsed line-by-line looking for VEVENT blocks with SUMMARY and DTSTART/DUE fields.
/// No third-party iCal library is required.
/// </summary>
public sealed class ICalFeedImportService(HttpClient httpClient) : ICalendarImportService
{
    /// <inheritdoc/>
    public async Task<List<(string Title, DateTime DueDate)>> ImportEventsAsync(
        string icalUrl,
        CancellationToken ct = default)
    {
        var content = await httpClient.GetStringAsync(icalUrl, ct);
        return ParseEvents(content);
    }

    /// <summary>Parses an iCalendar document and extracts (Title, DueDate) pairs from VEVENT blocks.</summary>
    private static List<(string Title, DateTime DueDate)> ParseEvents(string icalContent)
    {
        var results = new List<(string Title, DateTime DueDate)>();

        // Unfold continuation lines (RFC 5545 §3.1: lines folded with CRLF + whitespace)
        var unfolded = icalContent
            .Replace("\r\n ", string.Empty)
            .Replace("\r\n\t", string.Empty)
            .Replace("\n ", string.Empty)
            .Replace("\n\t", string.Empty);

        var lines = unfolded.Split(["\r\n", "\n"], StringSplitOptions.None);

        var inEvent = false;
        string? summary = null;
        DateTime? eventDate = null;

        foreach (var line in lines)
        {
            if (line.Equals("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase))
            {
                inEvent = true;
                summary = null;
                eventDate = null;
                continue;
            }

            if (line.Equals("END:VEVENT", StringComparison.OrdinalIgnoreCase))
            {
                if (inEvent && summary is not null && eventDate is not null)
                    results.Add((summary, eventDate.Value));

                inEvent = false;
                continue;
            }

            if (!inEvent)
                continue;

            if (line.StartsWith("SUMMARY:", StringComparison.OrdinalIgnoreCase))
            {
                summary = UnescapeICalText(line["SUMMARY:".Length..]);
            }
            else if (line.StartsWith("DUE", StringComparison.OrdinalIgnoreCase)
                  || line.StartsWith("DTSTART", StringComparison.OrdinalIgnoreCase))
            {
                // Property may have parameters, e.g. DTSTART;VALUE=DATE:20240101 or DTSTART;TZID=...:20240101T120000
                var colonIndex = line.IndexOf(':');
                if (colonIndex >= 0)
                {
                    var dateValue = line[(colonIndex + 1)..].Trim();
                    if (TryParseICalDate(dateValue, out var parsed))
                        eventDate ??= parsed; // DUE takes priority if it appears first; otherwise DTSTART wins
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Attempts to parse an iCalendar DATE or DATE-TIME value.
    /// Supports <c>yyyyMMdd</c> (all-day) and <c>yyyyMMdd'T'HHmmss'Z'</c> (UTC) forms.
    /// </summary>
    private static bool TryParseICalDate(string value, out DateTime result)
    {
        result = default;

        // DATE-TIME UTC: 20240101T120000Z
        if (value.Length == 16 && value[8] == 'T' && value[15] == 'Z'
            && DateTime.TryParseExact(value, "yyyyMMdd'T'HHmmss'Z'",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out result))
            return true;

        // DATE-TIME local: 20240101T120000
        if (value.Length == 15 && value[8] == 'T'
            && DateTime.TryParseExact(value, "yyyyMMdd'T'HHmmss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out result))
        {
            result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
            return true;
        }

        // DATE only: 20240101
        if (value.Length == 8
            && DateTime.TryParseExact(value, "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out result))
        {
            result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
            return true;
        }

        return false;
    }

    /// <summary>Reverses RFC 5545 text escaping (backslash sequences).</summary>
    private static string UnescapeICalText(string value) =>
        value
            .Replace("\\n", "\n")
            .Replace("\\,", ",")
            .Replace("\\;", ";")
            .Replace("\\\\", "\\");
}
