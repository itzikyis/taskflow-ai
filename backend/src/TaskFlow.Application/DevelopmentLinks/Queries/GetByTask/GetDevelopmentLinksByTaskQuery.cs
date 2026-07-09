using MediatR;
using TaskFlow.Application.DevelopmentLinks.Dtos;

namespace TaskFlow.Application.DevelopmentLinks.Queries.GetByTask;

/// <summary>Query returning all development links for a task.</summary>
public sealed record GetDevelopmentLinksByTaskQuery(Guid TaskId)
    : IRequest<IReadOnlyList<DevelopmentLinkDto>>;
