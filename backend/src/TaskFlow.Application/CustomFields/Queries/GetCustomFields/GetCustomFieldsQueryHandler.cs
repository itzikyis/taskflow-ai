using MediatR;
using TaskFlow.Application.CustomFields.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CustomFields.Queries.GetCustomFields;

/// <summary>Handles <see cref="GetCustomFieldsQuery"/>.</summary>
public sealed class GetCustomFieldsQueryHandler(ICustomFieldRepository repo)
    : IRequestHandler<GetCustomFieldsQuery, Result<List<CustomFieldDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<List<CustomFieldDto>>> Handle(GetCustomFieldsQuery request, CancellationToken ct)
    {
        var fields = await repo.GetByProjectAsync(request.ProjectId, ct);

        var dtos = fields
            .Select(f => new CustomFieldDto(f.Id, f.ProjectId, f.Name, f.FieldType, f.OptionsJson))
            .ToList();

        return Result<List<CustomFieldDto>>.Success(dtos);
    }
}
