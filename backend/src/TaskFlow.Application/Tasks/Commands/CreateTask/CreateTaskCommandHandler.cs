using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Tasks.Commands.CreateTask;

/// <summary>Handles <see cref="CreateTaskCommand"/>.</summary>
public sealed class CreateTaskCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        var taskResult = TaskItem.Create(
            request.Title,
            request.Description,
            request.Priority,
            request.CreatedByUserId);

        if (taskResult.IsFailure)
            return Result<Guid>.Failure(taskResult.Error);

        await taskRepository.AddAsync(taskResult.Value!, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(taskResult.Value!.Id);
    }
}
