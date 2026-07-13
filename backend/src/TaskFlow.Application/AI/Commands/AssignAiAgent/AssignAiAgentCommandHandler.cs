using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.AI.Commands.AssignAiAgent;

/// <summary>
/// Handles <see cref="AssignAiAgentCommand"/>: assigns the agent (v1 is read-only /
/// drafting — no state transitions) and posts a proposed-approach comment.
/// </summary>
public sealed class AssignAiAgentCommandHandler(
    ITaskRepository taskRepository,
    ICommentRepository commentRepository,
    IAiAssistantService ai,
    ILogger<AssignAiAgentCommandHandler> logger)
    : IRequestHandler<AssignAiAgentCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(AssignAiAgentCommand request, CancellationToken ct)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, ct);
        if (task is null)
            return Result<Guid>.Failure(TaskErrors.NotFound);

        task.AssignTo(AiAgent.Id);
        taskRepository.Update(task);

        string draft;
        try
        {
            draft = await ai.DraftTaskApproachAsync(task.Title, task.Description, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI agent could not draft an approach for task {TaskId}.", task.Id);
            draft = "🤖 AI Agent assigned. I couldn't draft an approach right now (the AI service is " +
                    "unavailable) — I'll pick this up once it's back.";
        }

        var commentResult = Comment.Create(task.Id, AiAgent.Id, draft);
        if (commentResult.IsFailure)
            return Result<Guid>.Failure(commentResult.Error);

        await commentRepository.AddAsync(commentResult.Value!, ct);
        // Shared DbContext: this persists both the assignment and the comment.
        await commentRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(commentResult.Value!.Id);
    }
}
