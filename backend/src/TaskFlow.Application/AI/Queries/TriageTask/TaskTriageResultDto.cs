namespace TaskFlow.Application.AI.Queries.TriageTask;

/// <summary>Result of an AI triage for a task given its content (title and description), without requiring an existing task ID.</summary>
/// <param name="SuggestedPriority">Suggested priority level: Low, Medium, High, or Urgent.</param>
/// <param name="Reasoning">The AI's explanation of the priority suggestion.</param>
/// <param name="PotentialDuplicates">Tasks in the project that may be duplicates of the candidate.</param>
public record TaskTriageResultDto(
    string SuggestedPriority,
    string Reasoning,
    List<PotentialDuplicateDto> PotentialDuplicates);

/// <summary>A single potential duplicate task detected by the triage process.</summary>
/// <param name="TaskId">ID of the existing task that may be a duplicate.</param>
/// <param name="Title">Title of the existing task.</param>
/// <param name="SimilarityScore">Similarity score in the range 0–1 (higher is more similar).</param>
public record PotentialDuplicateDto(Guid TaskId, string Title, double SimilarityScore);
