using FluentAssertions;
using TaskFlow.Application.Calendar;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Infrastructure.Tests.Integration;

/// <summary>Pure (no-DB) tests for <see cref="RfcCalendarFeedBuilder"/>.</summary>
public sealed class RfcCalendarFeedBuilderTests
{
    private readonly RfcCalendarFeedBuilder _sut = new();

    [Fact]
    public void Build_EmptyTasks_ReturnsValidEmptyCalendar()
    {
        var ical = _sut.Build([]);

        ical.Should().StartWith("BEGIN:VCALENDAR");
        ical.Should().Contain("VERSION:2.0");
        ical.TrimEnd().Should().EndWith("END:VCALENDAR");
        ical.Should().NotContain("BEGIN:VEVENT");
    }

    [Fact]
    public void Build_Task_EmitsAllDayVevent()
    {
        var id = Guid.NewGuid();
        var due = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);
        var ical = _sut.Build([new CalendarTask(id, "Ship release", "final QA", "InProgress", "High", due)]);

        ical.Should().Contain("BEGIN:VEVENT");
        ical.Should().Contain($"UID:{id}@taskflow-ai");
        ical.Should().Contain("DTSTART;VALUE=DATE:20260720");
        ical.Should().Contain("DTEND;VALUE=DATE:20260721");
        ical.Should().Contain("SUMMARY:[High] Ship release");
        ical.Should().Contain("Status: InProgress");
    }

    [Fact]
    public void Build_EscapesSpecialCharacters()
    {
        var ical = _sut.Build([new CalendarTask(
            Guid.NewGuid(), "Fix A, B; C", "line1\nline2", "Todo", "Low",
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))]);

        ical.Should().Contain("Fix A\\, B\\; C");
        ical.Should().Contain("line1\\nline2");
    }
}
