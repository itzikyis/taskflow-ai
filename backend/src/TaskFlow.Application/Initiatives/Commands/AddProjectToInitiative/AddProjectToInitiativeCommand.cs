using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Initiatives.Commands.AddProjectToInitiative;

/// <summary>Links an existing project to an initiative.</summary>
public sealed record AddProjectToInitiativeCommand(Guid InitiativeId, Guid ProjectId) : IRequest<Result>;
