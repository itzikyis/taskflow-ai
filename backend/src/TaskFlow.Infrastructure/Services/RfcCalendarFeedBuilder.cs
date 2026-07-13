using System.Text;
using TaskFlow.Application.Calendar;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>Builds an RFC 5545 iCalendar document with an all-day VEVENT per task due date.</summary>
public sealed class RfcCalendarFeedBuilder : ICalendarFeedBuilder
{
    /// <inheritdoc/>
    public string Build(IEnumerable<CalendarTask> tasks)
    {
        var sb = new StringBuilder();
        sb.Append("BEGIN:VCALENDAR\r\n");
        sb.Append("VERSION:2.0\r\n");
        sb.Append("PRODID:-//TaskFlow AI//Task Calendar//EN\r\n");
        sb.Append("CALSCALE:GREGORIAN\r\n");
        sb.Append("X-WR-CALNAME:TaskFlow AI\r\n");

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");

        foreach (var t in tasks)
        {
            var due = t.DueDate.Date;
            sb.Append("BEGIN:VEVENT\r\n");
            sb.Append($"UID:{t.Id}@taskflow-ai\r\n");
            sb.Append($"DTSTAMP:{stamp}\r\n");
            sb.Append($"DTSTART;VALUE=DATE:{due:yyyyMMdd}\r\n");
            sb.Append($"DTEND;VALUE=DATE:{due.AddDays(1):yyyyMMdd}\r\n");
            sb.Append($"SUMMARY:{Escape($"[{t.Priority}] {t.Title}")}\r\n");
            sb.Append($"DESCRIPTION:{Escape($"Status: {t.Status}. {t.Description}")}\r\n");
            sb.Append("END:VEVENT\r\n");
        }

        sb.Append("END:VCALENDAR\r\n");
        return sb.ToString();
    }

    // Escapes text per RFC 5545 §3.3.11 (backslash, semicolon, comma, newline).
    private static string Escape(string? value) =>
        (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");
}
