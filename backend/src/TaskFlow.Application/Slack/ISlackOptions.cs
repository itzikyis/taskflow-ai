namespace TaskFlow.Application.Slack;

/// <summary>
/// Runtime options for Slack integration, surfaced to the Application layer
/// without a direct dependency on <c>Microsoft.Extensions.Configuration</c>.
/// </summary>
public interface ISlackOptions
{
    /// <summary>
    /// The ID of the system user used as the creator of tasks that originate
    /// from Slack slash commands.  Must be a non-empty <see cref="Guid"/>.
    /// Configure via <c>Slack:SystemUserId</c> in <c>appsettings.json</c>.
    /// </summary>
    Guid SystemUserId { get; }

    /// <summary>
    /// The Slack signing secret used to verify inbound webhook requests.
    /// When <see langword="null"/> or empty, signature verification is skipped.
    /// Configure via <c>Slack:SigningSecret</c> in <c>appsettings.json</c>.
    /// </summary>
    string? SigningSecret { get; }
}
