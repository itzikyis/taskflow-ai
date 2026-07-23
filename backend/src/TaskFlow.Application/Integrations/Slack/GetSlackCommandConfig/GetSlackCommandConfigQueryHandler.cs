using MediatR;
using TaskFlow.Application.Slack;

namespace TaskFlow.Application.Integrations.Slack.GetSlackCommandConfig;

/// <summary>Handles <see cref="GetSlackCommandConfigQuery"/>.</summary>
public sealed class GetSlackCommandConfigQueryHandler(ISlackOptions slackOptions)
    : IRequestHandler<GetSlackCommandConfigQuery, SlackCommandConfigDto>
{
    /// <inheritdoc/>
    public Task<SlackCommandConfigDto> Handle(GetSlackCommandConfigQuery request, CancellationToken ct)
    {
        var isConfigured = !string.IsNullOrWhiteSpace(slackOptions.SigningSecret);
        return Task.FromResult(new SlackCommandConfigDto(isConfigured));
    }
}
