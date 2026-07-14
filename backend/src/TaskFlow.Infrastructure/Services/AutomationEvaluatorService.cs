using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// Evaluates enabled automation rules after a task event and fires matching actions.
/// Actions are best-effort: a failure in one rule never blocks the others.
/// </summary>
internal sealed class AutomationEvaluatorService(
    ApplicationDbContext db,
    INotificationRepository notificationRepo,
    ICommentRepository commentRepo,
    ILogger<AutomationEvaluatorService> logger) : IAutomationEvaluatorService
{
    public async Task EvaluateAsync(
        TaskItem task,
        AutomationTriggerType trigger,
        string triggerValue,
        CancellationToken ct)
    {
        var rules = await db.AutomationRules
            .Where(r => r.IsEnabled && r.TriggerType == trigger && r.TriggerValue == triggerValue)
            .ToListAsync(ct);

        foreach (var rule in rules)
        {
            try
            {
                await ApplyActionAsync(rule, task, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Automation rule {RuleId} action failed for task {TaskId}.", rule.Id, task.Id);
            }
        }
    }

    private async Task ApplyActionAsync(AutomationRule rule, TaskItem task, CancellationToken ct)
    {
        switch (rule.ActionType)
        {
            case AutomationActionType.SendNotification:
            {
                var recipientId = task.AssignedToUserId ?? task.CreatedByUserId;
                var notification = Notification.Create(
                    recipientId,
                    $"Automation: {rule.Name}",
                    rule.ActionValue.Replace("{task}", task.Title),
                    NotificationType.General,
                    task.Id);
                await notificationRepo.AddAsync(notification, ct);
                await notificationRepo.SaveChangesAsync(ct);
                break;
            }

            case AutomationActionType.PostComment:
            {
                var message = rule.ActionValue.Replace("{task}", task.Title);
                var commentResult = Comment.Create(task.Id, task.CreatedByUserId, $"🤖 Automation ({rule.Name}): {message}");
                if (commentResult.IsSuccess && commentResult.Value is not null)
                {
                    await commentRepo.AddAsync(commentResult.Value, ct);
                    await commentRepo.SaveChangesAsync(ct);
                }
                break;
            }

            case AutomationActionType.ChangeStatus:
            {
                if (Enum.TryParse<TaskItemStatus>(rule.ActionValue, out var newStatus))
                {
                    task.TransitionTo(newStatus);
                    db.Update(task);
                    await db.SaveChangesAsync(ct);
                }
                break;
            }
        }
    }
}
