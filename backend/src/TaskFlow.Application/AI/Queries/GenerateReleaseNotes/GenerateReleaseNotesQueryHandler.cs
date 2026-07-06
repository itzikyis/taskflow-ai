using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.GenerateReleaseNotes;

/// <summary>Handles <see cref="GenerateReleaseNotesQuery"/> by delegating to the AI assistant service.</summary>
public sealed class GenerateReleaseNotesQueryHandler(
    IAiAssistantService ai,
    ILogger<GenerateReleaseNotesQueryHandler> logger)
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
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI release notes generation failed: service is misconfigured.");
            return Result<ReleaseNotes>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI release notes generation failed due to an unexpected error.");
            return Result<ReleaseNotes>.Failure(AiErrors.Unavailable);
        }
    }
}
