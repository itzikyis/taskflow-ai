using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.SetTaskRecurrence;

/// <summary>Handles <see cref="SetTaskRecurrenceCommand"/>.</summary>
public sealed class SetTaskRecurrenceCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<SetTaskRecurrenceCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        SetTaskRecurrenceCommand request,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var result = task.SetRecurrence(request.Pattern, request.EndDate);
        if (result.IsFailure)
            return result;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}
