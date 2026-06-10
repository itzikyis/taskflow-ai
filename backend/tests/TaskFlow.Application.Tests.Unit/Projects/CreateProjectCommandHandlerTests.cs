using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Projects.Commands.CreateProject;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Tests.Unit.Projects;

public sealed class CreateProjectCommandHandlerTests
{
    private readonly IProjectRepository _repository = Substitute.For<IProjectRepository>();
    private readonly CreateProjectCommandHandler _sut;

    public CreateProjectCommandHandlerTests()
    {
        _sut = new CreateProjectCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithNewId()
    {
        var command = new CreateProjectCommand("My Project", "desc", Guid.NewGuid());
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(Arg.Any<Project>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsFailure()
    {
        var command = new CreateProjectCommand("", null, Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Project.NameRequired");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Project>(), Arg.Any<CancellationToken>());
    }
}
