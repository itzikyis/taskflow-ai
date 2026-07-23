using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Integrations.Slack;
using TaskFlow.Application.Integrations.Slack.DeleteSlackConfig;
using TaskFlow.Application.Integrations.Slack.GetSlackCommandConfig;
using TaskFlow.Application.Integrations.Slack.GetSlackConfig;
using TaskFlow.Application.Integrations.Slack.SaveSlackConfig;
using TaskFlow.Application.Integrations.Slack.SendSlackTest;

namespace TaskFlow.API.Controllers;

/// <summary>Slack Incoming Webhook integration settings.</summary>
[ApiController]
[Route("api/integrations/slack")]
[Authorize]
public sealed class SlackController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns whether the Slack slash-command signing secret is configured.</summary>
    [HttpGet("command-config")]
    [ProducesResponseType(typeof(SlackCommandConfigDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommandConfig(CancellationToken ct)
        => Ok(await mediator.Send(new GetSlackCommandConfigQuery(), ct));

    /// <summary>Gets the current Slack integration config (webhook URL masked).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(SlackConfigDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await mediator.Send(new GetSlackConfigQuery(), ct));

    /// <summary>Creates or updates the Slack integration config.</summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Save([FromBody] SaveSlackConfigCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsFailure ? BadRequest(result.Error) : NoContent();
    }

    /// <summary>Sends a test message to the configured Slack webhook.</summary>
    [HttpPost("test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Test(CancellationToken ct)
    {
        var result = await mediator.Send(new SendSlackTestCommand(), ct);
        return result.IsFailure ? BadRequest(result.Error) : NoContent();
    }

    /// <summary>Removes the Slack integration.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(CancellationToken ct)
    {
        await mediator.Send(new DeleteSlackConfigCommand(), ct);
        return NoContent();
    }
}
