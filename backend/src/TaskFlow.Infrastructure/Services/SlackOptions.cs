using Microsoft.Extensions.Configuration;
using TaskFlow.Application.Slack;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// Reads Slack-related configuration values from <c>appsettings.json</c> (or environment
/// variables) and exposes them through <see cref="ISlackOptions"/>.
/// </summary>
public sealed class SlackOptions(IConfiguration configuration) : ISlackOptions
{
    /// <inheritdoc/>
    public Guid SystemUserId =>
        Guid.TryParse(configuration["Slack:SystemUserId"], out var id) ? id : Guid.Empty;

    /// <inheritdoc/>
    public string? SigningSecret => configuration["Slack:SigningSecret"];
}
