namespace TaskFlow.Application.DuplicateDetection;

/// <summary>A candidate duplicate task with its similarity score (0–1).</summary>
public sealed record DuplicateMatch(Guid TaskId, string Title, double Score);
