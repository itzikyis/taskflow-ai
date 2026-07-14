using MediatR;
using TaskFlow.Application.Initiatives.Dtos;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Initiatives.Queries.GetInitiativeRoadmap;

/// <summary>Handles <see cref="GetInitiativeRoadmapQuery"/>.</summary>
public sealed class GetInitiativeRoadmapQueryHandler(
    IInitiativeRepository initiativeRepo) : IRequestHandler<GetInitiativeRoadmapQuery, IReadOnlyList<InitiativeDto>>
{
    public async Task<IReadOnlyList<InitiativeDto>> Handle(
        GetInitiativeRoadmapQuery request, CancellationToken ct)
    {
        var initiatives = await initiativeRepo.GetAllAsync(ct);

        return initiatives
            .OrderByDescending(i => (int)i.Priority)
            .Select(i => new InitiativeDto(
                i.Id, i.Name, i.Description,
                i.Status.ToString(), i.Priority.ToString(),
                i.Labels, i.StartDate, i.TargetDate,
                i.CreatedByUserId, i.CreatedAt,
                i.ProjectIds,
                TotalTasks: 0,   // computed client-side via project data
                CompletedTasks: 0))
            .ToList();
    }
}
