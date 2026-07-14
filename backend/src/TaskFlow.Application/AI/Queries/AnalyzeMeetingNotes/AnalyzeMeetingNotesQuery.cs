using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.AnalyzeMeetingNotes;

/// <summary>Requests AI analysis of raw meeting notes into a structured summary and action items.</summary>
public sealed record AnalyzeMeetingNotesQuery(
    string Transcript,
    IReadOnlyList<string> ParticipantNames)
    : IRequest<Result<MeetingNotesResult>>;
