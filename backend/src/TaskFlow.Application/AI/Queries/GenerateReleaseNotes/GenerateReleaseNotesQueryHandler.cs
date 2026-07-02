using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.GenerateReleaseNotes;

/// <summary>Handles <see cref="GenerateReleaseNotesQuery"/> by delegating to the AI assistant service.</summary>
public sealed class GenerateReleaseNotesQueryHandler(IAiAssistantService ai)
    : IRequestHandler<GenerateReleaseNotesQuery, Result<ReleaseNotes>>
{
    /// <inheritdoc/>
    public async Task<Result<ReleaseNotes>> Handle(GenerateReleaseNotesQuery request, CancellationToken ct)
    {
        try
        {
            var tasks = request.CompletedTasks
                .Select(t => (t.Title, t.Description, t.Priority));

            var notes = await ai.GenerateReleaseNotesAsync(request.Version, tasks, ct);
            return Result<ReleaseNotes>.Success(notes);
        }
        catch (Exception)
        {
            return Result<ReleaseNotes>.Failure(new Error("AI.Unavailable", "AI service unavailable."));
        }
    }
}
