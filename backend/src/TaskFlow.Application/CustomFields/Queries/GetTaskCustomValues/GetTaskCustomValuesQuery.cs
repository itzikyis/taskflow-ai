using MediatR;
using TaskFlow.Application.CustomFields.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CustomFields.Queries.GetTaskCustomValues;

/// <summary>Returns all custom field values set on the given task.</summary>
/// <param name="TaskId">ID of the task to query.</param>
public sealed record GetTaskCustomValuesQuery(Guid TaskId) : IRequest<Result<List<CustomFieldValueDto>>>;
