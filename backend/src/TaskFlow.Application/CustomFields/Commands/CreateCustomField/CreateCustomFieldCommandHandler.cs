using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.CustomFields.Commands.CreateCustomField;

/// <summary>Handles <see cref="CreateCustomFieldCommand"/>.</summary>
public sealed class CreateCustomFieldCommandHandler(ICustomFieldRepository repo)
    : IRequestHandler<CreateCustomFieldCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(CreateCustomFieldCommand request, CancellationToken ct)
    {
        var result = CustomField.Create(
            request.ProjectId,
            request.Name,
            request.FieldType,
            request.OptionsJson);

        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        await repo.AddAsync(result.Value!, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value!.Id);
    }
}
