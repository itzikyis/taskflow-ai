using FluentAssertions;
using TaskFlow.Application.Tasks.Commands.CreateTask;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Tasks;

/// <summary>Validation rules guarding task creation (issue #38: out-of-range priority).</summary>
public sealed class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _sut = new();

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Medium)]
    [InlineData(TaskPriority.High)]
    [InlineData(TaskPriority.Critical)]
    public void Validate_ValidPriority_Passes(TaskPriority priority)
    {
        var command = new CreateTaskCommand("A valid title", null, priority, Guid.NewGuid());

        _sut.Validate(command).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void Validate_OutOfRangePriority_Fails(int rawPriority)
    {
        var command = new CreateTaskCommand(
            "A valid title", null, (TaskPriority)rawPriority, Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTaskCommand.Priority));
    }
}
