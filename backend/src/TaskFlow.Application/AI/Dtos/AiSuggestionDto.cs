namespace TaskFlow.Application.AI.Dtos;

/// <summary>AI-generated text suggestion.</summary>
public sealed record AiSuggestionDto(string Suggestion);

/// <summary>AI-generated due date suggestion with explanation.</summary>
public sealed record AiDueDateSuggestionDto(string SuggestedDate, string Reasoning);
