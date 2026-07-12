using MediatR;
using TaskFlow.Application.Dependencies.Dtos;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Dependencies.Queries.GetAllDependencies;

/// <summary>Handles <see cref="GetAllDependenciesQuery"/>.</summary>
public sealed class GetAllDependenciesQueryHandler(ITaskDependencyRepository repo)
    : IRequestHandler<GetAllDependenciesQuery, IReadOnlyList<DependencyEdgeDto>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DependencyEdgeDto>> Handle(
        GetAllDependenciesQuery request, CancellationToken ct)
    {
        var all = await repo.GetAllAsync(ct);
        return all.Select(d => new DependencyEdgeDto(d.Id, d.TaskId, d.BlockedByTaskId)).ToList();
    }
}
