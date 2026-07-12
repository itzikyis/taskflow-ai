using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TimeTracking.Commands.DeleteTimeEntry;

/// <summary>Handles <see cref="DeleteTimeEntryCommand"/>.</summary>
public sealed class DeleteTimeEntryCommandHandler(ITimeEntryRepository repo)
    : IRequestHandler<DeleteTimeEntryCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteTimeEntryCommand request, CancellationToken ct)
    {
        var entry = await repo.GetByIdAsync(request.EntryId, ct);
        if (entry is null)
            return Result.Failure(TimeEntryErrors.NotFound);

        if (entry.UserId != request.RequesterId)
            return Result.Failure(new Error("TimeEntry.NotOwner", "Only the author can delete this time entry."));

        repo.Remove(entry);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
