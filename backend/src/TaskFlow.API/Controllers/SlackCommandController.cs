using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Slack;
using TaskFlow.Application.Slack.Commands.CreateTaskFromSlack;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Handles inbound Slack slash-command webhooks.
/// Slack calls this endpoint directly with <c>application/x-www-form-urlencoded</c> payloads,
/// so no JWT auth middleware is applied; an optional HMAC-SHA256 signing-secret check is
/// performed instead when <c>Slack:SigningSecret</c> is present in configuration.
/// </summary>
[ApiController]
[Route("api/slack")]
[AllowAnonymous]
public sealed class SlackCommandController(IMediator mediator, ISlackOptions slackOptions) : ControllerBase
{
    /// <summary>
    /// Handles the <c>/taskflow create &lt;title&gt;</c> Slack slash command.
    /// Slack posts <c>application/x-www-form-urlencoded</c> with fields:
    /// <c>command</c>, <c>text</c>, <c>user_id</c>, <c>user_name</c>, <c>team_id</c>, <c>channel_id</c>.
    /// Returns an ephemeral Slack response confirming the task was created.
    /// </summary>
    [HttpPost("command")]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleCommand(
        [FromForm] string? command,
        [FromForm] string? text,
        [FromForm(Name = "user_id")] string? userId,
        [FromForm(Name = "user_name")] string? userName,
        [FromForm(Name = "team_id")] string? teamId,
        [FromForm(Name = "channel_id")] string? channelId,
        CancellationToken ct)
    {
        // Optional Slack signing-secret verification.
        if (!await VerifySlackSignatureAsync())
            return Unauthorized(SlackEphemeral(":lock: Request signature verification failed."));

        var title = text?.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            return Ok(SlackEphemeral(
                ":information_source: Please provide a task title. Usage: `/taskflow create <title>`"));
        }

        var cmd = new CreateTaskFromSlackCommand(
            Title: title,
            SlackUserId: userId ?? string.Empty,
            SlackUserName: userName ?? string.Empty,
            ChannelId: channelId ?? string.Empty);

        var result = await mediator.Send(cmd, ct);

        if (result.IsFailure)
        {
            return Ok(SlackEphemeral(
                $":warning: Could not create task: {result.Error.Description}"));
        }

        return Ok(SlackEphemeral(
            $":white_check_mark: Task created successfully: *{title}* (ID: `{result.Value}`)"));
    }

    /// <summary>
    /// Verifies the Slack request signature using HMAC-SHA256.
    /// Returns <see langword="true"/> when the signing secret is not configured (skip check)
    /// or when the signature is valid.
    /// </summary>
    private async Task<bool> VerifySlackSignatureAsync()
    {
        var signingSecret = slackOptions.SigningSecret;
        if (string.IsNullOrWhiteSpace(signingSecret))
            return true; // Signing secret not configured — skip verification.

        if (!Request.Headers.TryGetValue("X-Slack-Request-Timestamp", out var timestampValues)
            || !Request.Headers.TryGetValue("X-Slack-Signature", out var signatureValues))
            return false;

        var timestamp = timestampValues.ToString();
        var slackSignature = signatureValues.ToString();

        // Guard against replay attacks: reject requests older than 5 minutes.
        if (!long.TryParse(timestamp, out var ts))
            return false;

        var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ts;
        if (age > 300)
            return false;

        // Re-read the raw request body for signing (buffering enabled in Program.cs).
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();

        var sigBase = $"v0:{timestamp}:{rawBody}";
        var key = Encoding.UTF8.GetBytes(signingSecret);
        var data = Encoding.UTF8.GetBytes(sigBase);

        var hash = HMACSHA256.HashData(key, data);
        var computedSig = $"v0={Convert.ToHexString(hash).ToLowerInvariant()}";

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSig),
            Encoding.UTF8.GetBytes(slackSignature));
    }

    /// <summary>Builds a Slack ephemeral response object.</summary>
    private static object SlackEphemeral(string text) =>
        new { response_type = "ephemeral", text };
}
