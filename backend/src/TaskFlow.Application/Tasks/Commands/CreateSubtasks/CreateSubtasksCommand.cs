using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.CreateSubtasks;

/// <summary>A single subtask to create under a parent task.</summary>
public sealed record NewSubtaskInput(string Title, string? Description);

/// <summary>Command to create one or more subtasks under an existing parent task.</summary>
public sealed record CreateSubtasksCommand(
    Guid ParentTaskId,
    IReadOnlyList<NewSubtaskInput> Subtasks,
    Guid CreatedByUserId) : IRequest<Result<IReadOnlyList<Guid>>>;
