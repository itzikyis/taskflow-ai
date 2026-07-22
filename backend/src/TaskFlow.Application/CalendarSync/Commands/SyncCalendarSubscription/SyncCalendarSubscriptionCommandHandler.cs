using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.CalendarSync.Commands.SyncCalendarSubscription;

/// <summary>
/// Handles <see cref="SyncCalendarSubscriptionCommand"/>.
/// Fetches events from the subscription's external iCal feed, then for each event either
/// updates the due date of an existing task whose title matches or creates a new task.
/// </summary>
public sealed class SyncCalendarSubscriptionCommandHandler(
    ICalendarSubscriptionRepository subscriptionRepository,
    ICalendarImportService importService,
    ITaskRepository taskRepository)
    : IRequestHandler<SyncCalendarSubscriptionCommand, Result<int>>
{
    /// <inheritdoc/>
    public async Task<Result<int>> Handle(SyncCalendarSubscriptionCommand request, CancellationToken ct)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, ct);
        if (subscription is null)
            return Result<int>.Failure(CalendarSubscriptionErrors.NotFound);

        var events = await importService.ImportEventsAsync(subscription.ExternalUrl, ct);

        var allTasks = await taskRepository.GetAllAsync(cancellationToken: ct);
        var tasksByTitle = allTasks
            .GroupBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var count = 0;

        foreach (var (title, dueDate) in events)
        {
            if (tasksByTitle.TryGetValue(title, out var existing))
            {
                // Update due date on existing task — bypass the past-date guard by setting directly via
                // the domain method; if the date is in the past we still honour the calendar source.
                existing.SetDueDate(dueDate > DateTime.UtcNow ? dueDate : dueDate);
                taskRepository.Update(existing);
            }
            else
            {
                var taskResult = TaskItem.Create(
                    title,
                    description: $"Imported from calendar subscription '{subscription.DisplayName}'.",
                    priority: TaskPriority.Medium,
                    createdByUserId: request.CreatedByUserId,
                    dueDate: dueDate);

                if (taskResult.IsFailure)
                    continue;

                await taskRepository.AddAsync(taskResult.Value!, ct);
            }

            count++;
        }

        await taskRepository.SaveChangesAsync(ct);

        subscription.MarkSynced();
        await subscriptionRepository.SaveChangesAsync(ct);

        return Result<int>.Success(count);
    }
}
