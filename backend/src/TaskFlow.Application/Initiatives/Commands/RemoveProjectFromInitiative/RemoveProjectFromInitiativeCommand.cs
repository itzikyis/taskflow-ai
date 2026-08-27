using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Initiatives.Commands.RemoveProjectFromInitiative;

/// <summary>Removes the link between a project and an initiative.</summary>
public sealed record RemoveProjectFromInitiativeCommand(Guid InitiativeId, Guid ProjectId) : IRequest<Result>;
