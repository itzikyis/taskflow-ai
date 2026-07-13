using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.ActivityLogs.Commands.LogActivity;

/// <summary>Handles <see cref="LogActivityCommand"/> by persisting a new activity log entry.</summary>
public sealed class LogActivityCommandHandler(
    IActivityLogRepository repository,
    ISlackIntegrationRepository slackRepository,
    IExternalNotificationService externalNotifier,
    ILogger<LogActivityCommandHandler> logger)
    : IRequestHandler<LogActivityCommand, Result<Guid>>
{
    /// <summary>Creates an activity log entry, persists it, and best-effort forwards it to Slack.</summary>
    public async Task<Result<Guid>> Handle(LogActivityCommand request, CancellationToken cancellationToken)
    {
        var log = ActivityLog.Create(
            request.ActorId,
            request.Action,
            request.EntityType,
            request.EntityId,
            request.EntityName,
            request.ProjectId,
            request.Metadata);

        await repository.AddAsync(log, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await TryForwardToSlackAsync(request, cancellationToken);

        return Result<Guid>.Success(log.Id);
    }

    private async Task TryForwardToSlackAsync(LogActivityCommand request, CancellationToken ct)
    {
        try
        {
            var slack = await slackRepository.GetAsync(ct);
            if (slack is null || !slack.Enabled || string.IsNullOrWhiteSpace(slack.WebhookUrl))
                return;

            if (!ShouldForward(slack, request.Action))
                return;

            var actor = request.ActorId.ToString()[..8];
            var name = string.IsNullOrWhiteSpace(request.EntityName) ? request.EntityType : request.EntityName;
            var text = $":taskflow: *{Humanize(request.Action)}* — {request.EntityType} \"{name}\" by `{actor}`";

            await externalNotifier.SendAsync(slack.WebhookUrl, text, ct);
        }
        catch (Exception ex)
        {
            // Slack delivery must never break the main operation.
            logger.LogWarning(ex, "Failed to forward activity to Slack.");
        }
    }

    private static bool ShouldForward(SlackIntegration slack, ActivityAction action) => action switch
    {
        ActivityAction.Created => slack.ForwardCreated,
        ActivityAction.StatusChanged => slack.ForwardStatusChanged,
        ActivityAction.CommentAdded => slack.ForwardComments,
        _ => slack.ForwardOther,
    };

    private static string Humanize(ActivityAction action) => action switch
    {
        ActivityAction.Created => "Created",
        ActivityAction.Updated => "Updated",
        ActivityAction.Deleted => "Deleted",
        ActivityAction.StatusChanged => "Status changed",
        ActivityAction.CommentAdded => "Comment added",
        _ => action.ToString(),
    };
}
