namespace TaskFlow.Application.AI;

/// <summary>The result of an AI-based story point estimation.</summary>
/// <param name="Points">Estimated story points on the Fibonacci scale (1, 2, 3, 5, 8, 13).</param>
/// <param name="Reasoning">One-sentence explanation of the estimate.</param>
public sealed record StoryPointEstimate(int Points, string Reasoning);
