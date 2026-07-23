namespace TaskFlow.Application.AI.Queries.GetStatusDigest;

/// <summary>AI-generated project status digest covering a rolling time window.</summary>
/// <param name="PeriodLabel">Human-readable label for the period, e.g. "Last 7 days".</param>
/// <param name="Completed">Titles of tasks completed within the period.</param>
/// <param name="InProgress">Titles of tasks currently in progress.</param>
/// <param name="Blockers">Titles of overdue or blocked tasks requiring attention.</param>
/// <param name="AiNarrative">3-5 sentence AI-written narrative summarising project health.</param>
/// <param name="HealthStatus">Overall project health: "Healthy", "At Risk", or "Critical".</param>
public sealed record StatusDigestDto(
    string PeriodLabel,
    List<string> Completed,
    List<string> InProgress,
    List<string> Blockers,
    string AiNarrative,
    string HealthStatus);
