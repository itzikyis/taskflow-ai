using TaskFlow.Application.DuplicateDetection;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Detects likely duplicate tasks by comparing a candidate task's text against
/// existing tasks. The default implementation uses text similarity; it can be
/// swapped for an embedding-based implementation without changing callers.
/// </summary>
public interface IDuplicateTaskDetectionService
{
    /// <summary>
    /// Returns existing tasks whose similarity to the candidate is at or above
    /// <paramref name="threshold"/>, ordered by descending score.
    /// </summary>
    /// <param name="candidateTitle">Title of the task being created.</param>
    /// <param name="candidateDescription">Optional description of the task being created.</param>
    /// <param name="existing">Existing tasks to compare against (id, title, description).</param>
    /// <param name="threshold">Minimum similarity score in the range 0–1.</param>
    IReadOnlyList<DuplicateMatch> FindDuplicates(
        string candidateTitle,
        string? candidateDescription,
        IEnumerable<(Guid Id, string Title, string? Description)> existing,
        double threshold = 0.35);
}
