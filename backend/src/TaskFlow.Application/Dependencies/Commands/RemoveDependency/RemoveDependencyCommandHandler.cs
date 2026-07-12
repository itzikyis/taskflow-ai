using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Dependencies.Commands.RemoveDependency;

/// <summary>Handles <see cref="RemoveDependencyCommand"/>.</summary>
public sealed class RemoveDependencyCommandHandler(ITaskDependencyRepository repo)
    : IRequestHandler<RemoveDependencyCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RemoveDependencyCommand request, CancellationToken ct)
    {
        var dependency = await repo.GetByIdAsync(request.DependencyId, ct);
        if (dependency is null)
            return Result.Failure(TaskDependencyErrors.NotFound);

        repo.Remove(dependency);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
