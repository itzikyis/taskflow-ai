using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.TriageTask;

/// <summary>
/// Requests AI-powered smart triage for a task that has not yet been persisted.
/// Accepts the task's content directly so it can be called from the "Create Task" flow before saving.
/// </summary>
/// <param name="Title">Title of the task to triage.</param>
/// <param name="Description">Optional description of the task.</param>
/// <param name="ProjectId">
/// Project to scope duplicate detection against. When the repository has no per-project
/// filter, all tasks are compared and the projectId is passed through for future use.
/// </param>
public sealed record TriageByContentQuery(
    string Title,
    string? Description,
    Guid ProjectId)
    : IRequest<Result<TaskTriageResultDto>>;
