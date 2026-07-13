using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Integrations.Slack.SaveSlackConfig;

/// <summary>Creates or updates the Slack integration config.</summary>
public sealed record SaveSlackConfigCommand(
    string WebhookUrl,
    bool Enabled,
    bool ForwardCreated,
    bool ForwardStatusChanged,
    bool ForwardComments,
    bool ForwardOther) : IRequest<Result>;
