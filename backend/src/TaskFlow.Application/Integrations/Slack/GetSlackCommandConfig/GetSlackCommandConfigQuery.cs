using MediatR;

namespace TaskFlow.Application.Integrations.Slack.GetSlackCommandConfig;

/// <summary>Returns whether the Slack slash-command signing secret is configured.</summary>
public sealed record GetSlackCommandConfigQuery : IRequest<SlackCommandConfigDto>;
