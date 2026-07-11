namespace TaskFlow.Application.AI;

/// <summary>An AI-suggested subtask produced when breaking down a larger task.</summary>
public sealed record SubtaskSuggestion(string Title, string? Description);
