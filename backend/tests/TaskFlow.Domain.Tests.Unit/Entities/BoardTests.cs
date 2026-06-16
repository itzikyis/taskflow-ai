using FluentAssertions;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Unit.Entities;

public sealed class BoardTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = Board.Create("Sprint Board", Guid.NewGuid());
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Sprint Board");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ReturnsNameRequired(string name)
    {
        var result = Board.Create(name, Guid.NewGuid());
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.NameRequired");
    }

    [Fact]
    public void Create_WithNameOver100Chars_ReturnsNameTooLong()
    {
        var result = Board.Create(new string('x', 101), Guid.NewGuid());
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.NameTooLong");
    }

    [Fact]
    public void Create_RaisesBoardCreatedEvent()
    {
        var result = Board.Create("Board", Guid.NewGuid());
        result.Value!.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddColumn_WithValidInputs_AddsColumn()
    {
        var board = Board.Create("Board", Guid.NewGuid()).Value!;
        var result = board.AddColumn("Todo", 0);
        result.IsSuccess.Should().BeTrue();
        board.Columns.Should().HaveCount(1);
        board.Columns[0].Name.Should().Be("Todo");
    }

    [Fact]
    public void AddColumn_WithDuplicateOrder_ReturnsDuplicateColumnOrder()
    {
        var board = Board.Create("Board", Guid.NewGuid()).Value!;
        board.AddColumn("Todo", 0);
        var result = board.AddColumn("Other", 0);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.DuplicateColumnOrder");
    }

    [Fact]
    public void RemoveColumn_ExistingColumn_RemovesItAndSucceeds()
    {
        var board = Board.Create("Board", Guid.NewGuid()).Value!;
        var col = board.AddColumn("Todo", 0).Value!;
        var result = board.RemoveColumn(col.Id);
        result.IsSuccess.Should().BeTrue();
        board.Columns.Should().BeEmpty();
    }

    [Fact]
    public void RemoveColumn_NonExistentColumn_ReturnsColumnNotFound()
    {
        var board = Board.Create("Board", Guid.NewGuid()).Value!;
        var result = board.RemoveColumn(Guid.NewGuid());
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.ColumnNotFound");
    }
}
