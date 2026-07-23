namespace TaskFlow.Application.AI.Queries.GetDashboardInsights;

/// <summary>AI-generated narrative summary of a project dashboard.</summary>
/// <param name="Narrative">2-4 sentence natural-language summary of the project health.</param>
/// <param name="Highlights">Bullet-point insights (e.g. blockers, overdue tasks, velocity).</param>
/// <param name="HealthStatus">Overall health: "Healthy", "At Risk", or "Critical".</param>
public sealed record DashboardInsightsDto(
    string Narrative,
    List<string> Highlights,
    string HealthStatus);
