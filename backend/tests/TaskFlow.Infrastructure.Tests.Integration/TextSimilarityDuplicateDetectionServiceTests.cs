using FluentAssertions;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Infrastructure.Tests.Integration;

/// <summary>Pure (no-DB) tests for <see cref="TextSimilarityDuplicateDetectionService"/>.</summary>
public sealed class TextSimilarityDuplicateDetectionServiceTests
{
    private readonly TextSimilarityDuplicateDetectionService _sut = new();

    [Fact]
    public void FindDuplicates_FlagsNearDuplicateAboveThreshold()
    {
        var existing = new[]
        {
            (Guid.NewGuid(), "Login page throws error for special characters in email", (string?)"authentication bug"),
            (Guid.NewGuid(), "Design marketing landing page", (string?)null),
        };

        var matches = _sut.FindDuplicates(
            "Fix login error with special character emails",
            "users cannot authenticate",
            existing);

        matches.Should().NotBeEmpty();
        matches[0].Title.Should().Contain("Login page throws error");
        matches[0].Score.Should().BeGreaterThan(0.35);
    }

    [Fact]
    public void FindDuplicates_ReturnsEmptyForUnrelatedTasks()
    {
        var existing = new[]
        {
            (Guid.NewGuid(), "Upgrade PostgreSQL to version 17", (string?)null),
        };

        var matches = _sut.FindDuplicates("Design onboarding illustrations", null, existing);

        matches.Should().BeEmpty();
    }

    [Fact]
    public void FindDuplicates_OrdersByDescendingScore()
    {
        var strong = Guid.NewGuid();
        var weak = Guid.NewGuid();
        var existing = new[]
        {
            (weak, "Update dashboard widget colors", (string?)null),
            (strong, "Export invoices to PDF from billing", (string?)"invoice pdf export"),
        };

        var matches = _sut.FindDuplicates("Add invoice PDF export to billing", "export invoices", existing);

        matches.Should().NotBeEmpty();
        matches[0].TaskId.Should().Be(strong);
    }
}
