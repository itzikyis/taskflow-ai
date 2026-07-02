using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.GenerateReleaseNotes;

/// <summary>Query that requests AI-generated release notes for a given version.</summary>
/// <param name="Version">The version identifier (e.g. "1.0.0").</param>
/// <param name="CompletedTasks">Tasks completed in this release.</param>
public sealed record GenerateReleaseNotesQuery(
    string Version,
    IReadOnlyList<CompletedTaskSummary> CompletedTasks) : IRequest<Result<ReleaseNotes>>;

/// <summary>A summarised completed task used as input for release note generation.</summary>
/// <param name="Title">Task title.</param>
/// <param name="Description">Optional task description.</param>
/// <param name="Priority">Task priority (e.g. "High", "Medium", "Low").</param>
public sealed record CompletedTaskSummary(string Title, string? Description, string Priority);
