using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.CustomFields.Commands.SetCustomFieldValue;

/// <summary>Handles <see cref="SetCustomFieldValueCommand"/>.</summary>
public sealed class SetCustomFieldValueCommandHandler(ICustomFieldRepository repo)
    : IRequestHandler<SetCustomFieldValueCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(SetCustomFieldValueCommand request, CancellationToken ct)
    {
        var field = await repo.GetByIdAsync(request.CustomFieldId, ct);
        if (field is null)
            return Result.Failure(CustomFieldErrors.NotFound);

        var existing = (await repo.GetValuesByTaskAsync(request.TaskId, ct))
            .FirstOrDefault(v => v.CustomFieldId == request.CustomFieldId);

        if (existing is not null)
        {
            existing.SetValue(request.Value);
            await repo.UpsertValueAsync(existing, ct);
        }
        else
        {
            var newValue = CustomFieldValue.Create(request.TaskId, request.CustomFieldId, request.Value);
            await repo.UpsertValueAsync(newValue, ct);
        }

        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
