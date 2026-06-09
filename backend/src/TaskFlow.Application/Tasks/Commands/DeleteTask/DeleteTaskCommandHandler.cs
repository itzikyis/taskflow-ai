using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.DeleteTask;

/// <summary>Handles <see cref="DeleteTaskCommand"/>.</summary>
internal sealed class DeleteTaskCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<DeleteTaskCommand, Result>
{
    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        taskRepository.Remove(task);
        await taskRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}
