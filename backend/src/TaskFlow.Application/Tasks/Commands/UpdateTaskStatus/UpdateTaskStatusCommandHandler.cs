using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.UpdateTaskStatus;

/// <summary>Handles <see cref="UpdateTaskStatusCommand"/>.</summary>
internal sealed class UpdateTaskStatusCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<UpdateTaskStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var result = task.TransitionTo(request.NewStatus);
        if (result.IsFailure)
            return result;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}
