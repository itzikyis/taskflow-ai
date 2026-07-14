using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Initiatives.Commands.DeleteInitiative;

/// <summary>Permanently removes an initiative.</summary>
public sealed record DeleteInitiativeCommand(Guid Id) : IRequest<Result>;
