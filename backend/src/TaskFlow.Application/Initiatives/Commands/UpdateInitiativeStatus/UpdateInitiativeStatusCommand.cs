using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Initiatives.Commands.UpdateInitiativeStatus;

/// <summary>Changes the lifecycle status of an initiative.</summary>
public sealed record UpdateInitiativeStatusCommand(Guid Id, InitiativeStatus Status) : IRequest<Result>;
