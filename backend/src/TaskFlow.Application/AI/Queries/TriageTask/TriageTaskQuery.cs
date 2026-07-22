using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.TriageTask;

/// <summary>Requests an AI-powered triage of a newly created task, suggesting an assignee, priority, and duplicate flags.</summary>
public sealed record TriageTaskQuery(
    /// <summary>The ID of the task to triage.</summary>
    Guid TaskId,
    /// <summary>The ID of the project the task belongs to.</summary>
    Guid ProjectId)
    : IRequest<Result<TriageTaskDto>>;
