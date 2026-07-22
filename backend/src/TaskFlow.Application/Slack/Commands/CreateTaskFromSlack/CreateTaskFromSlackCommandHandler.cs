using MediatR;
using TaskFlow.Application.ActivityLogs.Commands.LogActivity;
using TaskFlow.Application.AuditTrail.Commands.RecordAudit;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Slack.Commands.CreateTaskFromSlack;

/// <summary>Handles <see cref="CreateTaskFromSlackCommand"/> by creating a <see cref="Domain.Entities.TaskItem"/>.</summary>
public sealed class CreateTaskFromSlackCommandHandler(
    ITaskRepository taskRepository,
    ISlackOptions slackOptions,
    IMediator mediator)
    : IRequestHandler<CreateTaskFromSlackCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        CreateTaskFromSlackCommand request,
        CancellationToken cancellationToken)
    {
        if (slackOptions.SystemUserId == Guid.Empty)
            return Result<Guid>.Failure(SlackErrors.SystemUserNotConfigured);

        var taskResult = Domain.Entities.TaskItem.Create(
            request.Title,
            description: $"Created from Slack by @{request.SlackUserName} in channel {request.ChannelId}.",
            TaskPriority.Medium,
            slackOptions.SystemUserId);

        if (taskResult.IsFailure)
            return Result<Guid>.Failure(taskResult.Error);

        await taskRepository.AddAsync(taskResult.Value!, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await mediator.Send(new LogActivityCommand(
                slackOptions.SystemUserId,
                ActivityAction.Created,
                "Task",
                taskResult.Value!.Id,
                taskResult.Value!.Title),
                cancellationToken);
        }
        catch
        {
            // Logging failure must never break the main operation.
        }

        try
        {
            await mediator.Send(new RecordAuditCommand(
                slackOptions.SystemUserId,
                "Task",
                taskResult.Value!.Id,
                "Created"),
                cancellationToken);
        }
        catch
        {
            // Audit failure must never break the main operation.
        }

        return Result<Guid>.Success(taskResult.Value!.Id);
    }
}
