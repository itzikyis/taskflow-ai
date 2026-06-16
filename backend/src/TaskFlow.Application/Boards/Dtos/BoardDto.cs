namespace TaskFlow.Application.Boards.Dtos;

/// <summary>DTO for a board column.</summary>
public sealed record BoardColumnDto(Guid Id, string Name, int Order, int? WipLimit);

/// <summary>DTO for a board including its columns.</summary>
public sealed record BoardDto(
    Guid Id,
    string Name,
    Guid ProjectId,
    IReadOnlyList<BoardColumnDto> Columns,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
