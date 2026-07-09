using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.DevelopmentLinks.Commands.LinkDevelopment;

/// <summary>Handles <see cref="LinkDevelopmentCommand"/>.</summary>
public sealed class LinkDevelopmentCommandHandler(
    IDevelopmentLinkRepository repo,
    ITaskRepository taskRepository)
    : IRequestHandler<LinkDevelopmentCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(LinkDevelopmentCommand request, CancellationToken ct)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, ct);
        if (task is null)
            return Result<Guid>.Failure(TaskErrors.NotFound);

        var result = TaskDevelopmentLink.Create(
            request.TaskId, request.Repository, request.RefType,
            request.Title, request.Url, request.Status, request.ExternalId);

        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        await repo.AddAsync(result.Value!, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value!.Id);
    }
}
