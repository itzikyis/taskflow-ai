using MediatR;
using TaskFlow.Application.CustomFields.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CustomFields.Queries.GetTaskCustomValues;

/// <summary>Handles <see cref="GetTaskCustomValuesQuery"/>.</summary>
public sealed class GetTaskCustomValuesQueryHandler(ICustomFieldRepository repo)
    : IRequestHandler<GetTaskCustomValuesQuery, Result<List<CustomFieldValueDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<List<CustomFieldValueDto>>> Handle(GetTaskCustomValuesQuery request, CancellationToken ct)
    {
        var values = await repo.GetValuesByTaskAsync(request.TaskId, ct);

        // Collect the field IDs referenced by the values and load their definitions.
        var fieldIds = values.Select(v => v.CustomFieldId).Distinct().ToList();
        var fieldMap = new Dictionary<Guid, (string Name, string Type)>();

        foreach (var fieldId in fieldIds)
        {
            var field = await repo.GetByIdAsync(fieldId, ct);
            if (field is not null)
                fieldMap[field.Id] = (field.Name, field.FieldType);
        }

        var dtos = values
            .Where(v => fieldMap.ContainsKey(v.CustomFieldId))
            .Select(v => new CustomFieldValueDto(
                v.CustomFieldId,
                fieldMap[v.CustomFieldId].Name,
                fieldMap[v.CustomFieldId].Type,
                v.Value))
            .ToList();

        return Result<List<CustomFieldValueDto>>.Success(dtos);
    }
}
