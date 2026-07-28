using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.NaturalLanguageSearch;

/// <summary>
/// Query to perform a natural-language task search on behalf of a user.
/// </summary>
/// <param name="Query">The free-text query string entered by the user.</param>
/// <param name="UserId">The ID of the user performing the search (used to resolve "assigned to me" clauses).</param>
/// <param name="ProjectId">Optional project scope to restrict results.</param>
public sealed record NaturalLanguageSearchQuery(string Query, Guid UserId, Guid? ProjectId)
    : IRequest<Result<NaturalLanguageSearchResultDto>>;
