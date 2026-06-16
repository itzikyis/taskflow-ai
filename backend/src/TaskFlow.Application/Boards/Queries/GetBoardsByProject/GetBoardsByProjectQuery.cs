using MediatR;
using TaskFlow.Application.Boards.Dtos;

namespace TaskFlow.Application.Boards.Queries.GetBoardsByProject;

/// <summary>Returns all boards for a project.</summary>
public sealed record GetBoardsByProjectQuery(Guid ProjectId) : IRequest<IReadOnlyList<BoardDto>>;
