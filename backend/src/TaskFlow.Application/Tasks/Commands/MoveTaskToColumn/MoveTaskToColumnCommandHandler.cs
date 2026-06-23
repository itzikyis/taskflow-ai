using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.MoveTaskToColumn;

/// <summary>Handles <see cref="MoveTaskToColumnCommand"/>.</summary>
internal sealed class MoveTaskToColumnCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<MoveTaskToColumnCommand, Result>
{
    public async Task<Result> Handle(MoveTaskToColumnCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        task.MoveToColumn(request.ColumnId);
        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}
