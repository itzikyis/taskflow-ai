using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Commands.AssignAiAgent;

/// <summary>
/// Assigns the AI Agent to a task; the agent drafts a proposed-approach comment.
/// Returns the id of the comment the agent posted.
/// </summary>
public sealed record AssignAiAgentCommand(Guid TaskId) : IRequest<Result<Guid>>;
