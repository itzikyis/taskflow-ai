using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Slack.Commands.CreateTaskFromSlack;

/// <summary>
/// Command issued when a user runs <c>/taskflow create &lt;title&gt;</c> in Slack.
/// Returns the ID of the newly created task.
/// </summary>
public sealed record CreateTaskFromSlackCommand(
    /// <summary>The task title parsed from the slash-command text.</summary>
    string Title,
    /// <summary>The Slack user ID of the person who issued the command.</summary>
    string SlackUserId,
    /// <summary>The Slack display name of the person who issued the command.</summary>
    string SlackUserName,
    /// <summary>The Slack channel from which the command was issued.</summary>
    string ChannelId) : IRequest<Result<Guid>>;
