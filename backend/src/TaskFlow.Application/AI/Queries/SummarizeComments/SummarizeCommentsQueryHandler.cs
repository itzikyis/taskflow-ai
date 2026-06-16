using MediatR;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SummarizeComments;

/// <summary>Handles <see cref="SummarizeCommentsQuery"/>.</summary>
public sealed class SummarizeCommentsQueryHandler(IAiAssistantService ai)
    : IRequestHandler<SummarizeCommentsQuery, Result<AiSuggestionDto>>
{
    public async Task<Result<AiSuggestionDto>> Handle(SummarizeCommentsQuery request, CancellationToken ct)
    {
        if (request.Comments.Count == 0)
            return Result<AiSuggestionDto>.Failure(new Error("AI.NoComments", "At least one comment is required to summarize."));
        var summary = await ai.SummarizeCommentsAsync(request.Comments, ct);
        return Result<AiSuggestionDto>.Success(new AiSuggestionDto(summary));
    }
}
