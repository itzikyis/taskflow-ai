using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Reporting.Dtos;
using TaskFlow.Application.Reporting.Queries.GetDashboardMetrics;

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
}
