namespace TaskFlow.Application.AI;

/// <summary>Copilot response to a natural-language question about project status.</summary>
public sealed record CopilotAnswer(string Answer, IReadOnlyList<string> ReferencedTaskIds);
