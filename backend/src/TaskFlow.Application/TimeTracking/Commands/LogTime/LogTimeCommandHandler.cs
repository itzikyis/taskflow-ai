using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.TimeTracking.Commands.LogTime;

/// <summary>Handles <see cref="LogTimeCommand"/>.</summary>
public sealed class LogTimeCommandHandler(
    ITimeEntryRepository repo,
    ITaskRepository taskRepository)
    : IRequestHandler<LogTimeCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(LogTimeCommand request, CancellationToken ct)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, ct);
        if (task is null)
            return Result<Guid>.Failure(TaskErrors.NotFound);

        var result = TimeEntry.Create(
            request.TaskId, request.UserId, request.Minutes, request.Note, request.LoggedAt);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        await repo.AddAsync(result.Value!, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value!.Id);
    }
}
