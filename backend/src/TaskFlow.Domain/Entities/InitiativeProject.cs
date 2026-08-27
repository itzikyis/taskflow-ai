namespace TaskFlow.Domain.Entities;

/// <summary>
/// Join entity linking an <see cref="Initiative"/> to a project by ID.
/// Keeps aggregate boundaries intact — only the project ID is stored, not a navigation to Project.
/// </summary>
public sealed class InitiativeProject
{
    private InitiativeProject() { } // EF Core

    /// <summary>Creates a new link.</summary>
    public InitiativeProject(Guid initiativeId, Guid projectId)
    {
        InitiativeId = initiativeId;
        ProjectId = projectId;
    }

    /// <summary>FK to the owning initiative.</summary>
    public Guid InitiativeId { get; private set; }

    /// <summary>ID of the linked project.</summary>
    public Guid ProjectId { get; private set; }
}
