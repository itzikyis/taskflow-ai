using FluentAssertions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Tests.Unit.Entities;

public sealed class ProjectTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessResult()
    {
        var result = Project.Create("My Project", "A description", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("My Project");
        result.Value.Description.Should().Be("A description");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ReturnsFailure(string name)
    {
        var result = Project.Create(name, null, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Project.NameRequired");
    }

    [Fact]
    public void Create_WithNameExceeding100Chars_ReturnsFailure()
    {
        var result = Project.Create(new string('x', 101), null, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Project.NameTooLong");
    }

    [Fact]
    public void Create_RaisesDomainEvent()
    {
        var result = Project.Create("Project", null, Guid.NewGuid());

        result.Value!.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Update_WithValidInputs_UpdatesNameAndDescription()
    {
        var project = Project.Create("Old Name", "Old desc", Guid.NewGuid()).Value!;

        var result = project.Update("New Name", "New desc");

        result.IsSuccess.Should().BeTrue();
        project.Name.Should().Be("New Name");
        project.Description.Should().Be("New desc");
        project.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithEmptyName_ReturnsFailure()
    {
        var project = Project.Create("Name", null, Guid.NewGuid()).Value!;

        var result = project.Update("", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Project.NameRequired");
    }
}
