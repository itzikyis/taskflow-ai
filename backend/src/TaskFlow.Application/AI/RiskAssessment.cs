namespace TaskFlow.Application.AI;

/// <summary>Risk level for a task or sprint.</summary>
public enum RiskLevel { OnTrack, AtRisk, Blocked }

/// <summary>AI-generated risk score for a single task.</summary>
public sealed record TaskRiskScore(
    Guid TaskId,
    string Title,
    RiskLevel Level,
    string Reason);

/// <summary>Sprint-wide risk summary produced by AI analysis.</summary>
public sealed record SprintRiskAssessment(
    IReadOnlyList<TaskRiskScore> Tasks,
    int OnTrackCount,
    int AtRiskCount,
    int BlockedCount,
    string Summary,
    IReadOnlyList<string> Recommendations);
