namespace TaskFlow.Application.AI;

/// <summary>
/// The AI Agent pseudo-user. It has a well-known, fixed id (no Users row) and can
/// be assigned to a task, where it drafts a proposed approach as a comment.
/// </summary>
public static class AiAgent
{
    /// <summary>The fixed identifier used as the AI agent's assignee/author id.</summary>
    public static readonly Guid Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <summary>Human-readable display name.</summary>
    public const string DisplayName = "AI Agent";
}
