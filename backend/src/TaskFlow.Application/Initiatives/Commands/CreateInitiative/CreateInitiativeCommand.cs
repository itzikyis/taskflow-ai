using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Initiatives.Commands.CreateInitiative;

/// <summary>Creates a new initiative.</summary>
public sealed record CreateInitiativeCommand(
    string Name,
    string Description,
    InitiativePriority Priority,
    string Labels,
    DateTime? StartDate,
    DateTime? TargetDate,
    Guid CreatedByUserId) : IRequest<Result<Guid>>;
