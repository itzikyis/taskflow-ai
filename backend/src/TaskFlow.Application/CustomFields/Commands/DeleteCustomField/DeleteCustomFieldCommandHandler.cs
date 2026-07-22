using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CustomFields.Commands.DeleteCustomField;

/// <summary>Handles <see cref="DeleteCustomFieldCommand"/>.</summary>
public sealed class DeleteCustomFieldCommandHandler(ICustomFieldRepository repo)
    : IRequestHandler<DeleteCustomFieldCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteCustomFieldCommand request, CancellationToken ct)
    {
        var field = await repo.GetByIdAsync(request.FieldId, ct);
        if (field is null)
            return Result.Failure(CustomFieldErrors.NotFound);

        await repo.RemoveAsync(field, ct);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
