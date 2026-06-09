using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.UpdateTask;

/// <summary>Handles <see cref="UpdateTaskCommand"/>.</summary>
internal sealed class UpdateTaskCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<UpdateTaskCommand, Result>
{
    public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var updateResult = task.UpdateDetails(request.Title, request.Description);
        if (updateResult.IsFailure)
            return updateResult;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}
