using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.AddColumn;

/// <summary>Adds a column to an existing board.</summary>
public sealed record AddColumnCommand(Guid BoardId, string Name, int Order, int? WipLimit) : IRequest<Result<Guid>>;
