using MediatR;
using TaskFlow.Application.DevelopmentLinks.Dtos;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.DevelopmentLinks.Queries.GetByTask;

/// <summary>Handles <see cref="GetDevelopmentLinksByTaskQuery"/>.</summary>
public sealed class GetDevelopmentLinksByTaskQueryHandler(IDevelopmentLinkRepository repo)
    : IRequestHandler<GetDevelopmentLinksByTaskQuery, IReadOnlyList<DevelopmentLinkDto>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DevelopmentLinkDto>> Handle(
        GetDevelopmentLinksByTaskQuery request, CancellationToken ct)
    {
        var links = await repo.GetByTaskIdAsync(request.TaskId, ct);

        return links.Select(l => new DevelopmentLinkDto(
            l.Id,
            l.TaskId,
            l.Repository,
            l.RefType.ToString(),
            l.Title,
            l.Url,
            l.Status.ToString(),
            l.ExternalId,
            l.CreatedAt,
            l.UpdatedAt)).ToList();
    }
}
