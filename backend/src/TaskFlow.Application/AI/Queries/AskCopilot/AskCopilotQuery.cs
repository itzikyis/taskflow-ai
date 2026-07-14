using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.AskCopilot;

/// <summary>A single task snapshot provided as context to the copilot.</summary>
public sealed record CopilotTaskContext(
    string Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    string? DueDate,
    int OpenBlockerCount,
    IReadOnlyList<string> RecentComments);

/// <summary>Asks the AI copilot a natural-language question about the project.</summary>
public sealed record AskCopilotQuery(
    string Question,
    IReadOnlyList<CopilotTaskContext> Tasks,
    IReadOnlyList<string> ConversationHistory)
    : IRequest<Result<CopilotAnswer>>;
