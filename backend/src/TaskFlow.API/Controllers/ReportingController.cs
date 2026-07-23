using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Reporting.Dtos;
using TaskFlow.Application.Reporting.Queries.GetDashboardMetrics;
using TaskFlow.Application.Reporting.Queries.GetProjectAnalytics;

namespace TaskFlow.API.Controllers;

/// <summary>Read-only aggregate reporting endpoints.</summary>
[ApiController]
[Route("api/reporting")]
[Authorize]
public sealed class ReportingController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns aggregate project analytics for the dashboard.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await mediator.Send(new GetDashboardMetricsQuery(), ct));

    /// <summary>Returns velocity and status breakdown analytics for a project.</summary>
    [HttpGet("analytics/{projectId:guid}")]
    [ProducesResponseType(typeof(ProjectAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProjectAnalytics(Guid projectId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProjectAnalyticsQuery(projectId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
