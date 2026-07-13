using TaskFlow.Application.Calendar;

namespace TaskFlow.Application.Interfaces;

/// <summary>Builds an iCalendar (RFC 5545) feed from tasks that have due dates.</summary>
public interface ICalendarFeedBuilder
{
    /// <summary>Returns a text/calendar VCALENDAR document for the given tasks.</summary>
    string Build(IEnumerable<CalendarTask> tasks);
}
