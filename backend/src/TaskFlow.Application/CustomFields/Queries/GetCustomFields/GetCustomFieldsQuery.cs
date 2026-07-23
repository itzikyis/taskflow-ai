using MediatR;
using TaskFlow.Application.CustomFields.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CustomFields.Queries.GetCustomFields;

/// <summary>Returns all custom field definitions for the given project.</summary>
/// <param name="ProjectId">ID of the project to query.</param>
public sealed record GetCustomFieldsQuery(Guid ProjectId) : IRequest<Result<List<CustomFieldDto>>>;
