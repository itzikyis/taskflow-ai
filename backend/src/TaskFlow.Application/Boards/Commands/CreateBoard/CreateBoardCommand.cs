using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.CreateBoard;

/// <summary>Creates a new Kanban board for a project.</summary>
public sealed record CreateBoardCommand(string Name, Guid ProjectId) : IRequest<Result<Guid>>;
