using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.AnalyzeMeetingNotes;

/// <summary>Handles <see cref="AnalyzeMeetingNotesQuery"/> by delegating to the AI assistant.</summary>
public sealed class AnalyzeMeetingNotesQueryHandler(
    IAiAssistantService ai,
    ILogger<AnalyzeMeetingNotesQueryHandler> logger)
    : IRequestHandler<AnalyzeMeetingNotesQuery, Result<MeetingNotesResult>>
{
    /// <inheritdoc/>
    public async Task<Result<MeetingNotesResult>> Handle(AnalyzeMeetingNotesQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Transcript))
            return Result<MeetingNotesResult>.Failure(new Error("MeetingNotes.EmptyTranscript", "Transcript cannot be empty."));

        try
        {
            var result = await ai.AnalyzeMeetingNotesAsync(request.Transcript, request.ParticipantNames, ct);
            return Result<MeetingNotesResult>.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI meeting notes failed: service is misconfigured.");
            return Result<MeetingNotesResult>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI meeting notes failed due to an unexpected error.");
            return Result<MeetingNotesResult>.Failure(AiErrors.Unavailable);
        }
    }
}
