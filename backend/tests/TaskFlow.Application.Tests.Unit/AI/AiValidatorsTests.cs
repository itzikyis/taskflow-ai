using FluentAssertions;
using TaskFlow.Application.AI.Queries.EstimateStoryPoints;
using TaskFlow.Application.AI.Queries.GenerateReleaseNotes;
using TaskFlow.Application.AI.Queries.SuggestSprintPlan;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AI;

/// <summary>Validation rules that guard the AI endpoints against bad input (issue #24).</summary>
public sealed class AiValidatorsTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Valid title", true)]
    public void EstimateStoryPoints_TitleRequired(string title, bool expectValid)
    {
        var result = new EstimateStoryPointsQueryValidator()
            .Validate(new EstimateStoryPointsQuery(title, null));

        result.IsValid.Should().Be(expectValid);
    }

    [Fact]
    public void SuggestSprintPlan_EmptyBacklog_IsInvalid()
    {
        var result = new SuggestSprintPlanQueryValidator()
            .Validate(new SuggestSprintPlanQuery([], 40));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(40, true)]
    public void SuggestSprintPlan_CapacityMustBePositive(int capacity, bool expectValid)
    {
        var query = new SuggestSprintPlanQuery(
            [new TaskSummary(Guid.NewGuid(), "Task", null, "Medium", "Todo")],
            capacity);

        var result = new SuggestSprintPlanQueryValidator().Validate(query);

        result.IsValid.Should().Be(expectValid);
    }

    [Fact]
    public void GenerateReleaseNotes_EmptyVersionOrTasks_IsInvalid()
    {
        var validator = new GenerateReleaseNotesQueryValidator();

        validator.Validate(new GenerateReleaseNotesQuery("", [new CompletedTaskSummary("t", null, "Low")]))
            .IsValid.Should().BeFalse();

        validator.Validate(new GenerateReleaseNotesQuery("1.0.0", []))
            .IsValid.Should().BeFalse();

        validator.Validate(new GenerateReleaseNotesQuery("1.0.0", [new CompletedTaskSummary("t", null, "Low")]))
            .IsValid.Should().BeTrue();
    }
}
