using MediatR;

namespace TaskFlow.Application.Integrations.Slack.GetSlackConfig;

/// <summary>Query returning the current Slack integration config (or an unconfigured default).</summary>
public sealed record GetSlackConfigQuery : IRequest<SlackConfigDto>;
