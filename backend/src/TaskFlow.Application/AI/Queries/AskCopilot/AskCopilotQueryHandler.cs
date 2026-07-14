using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.AskCopilot;

/// <summary>Handles <see cref="AskCopilotQuery"/> by delegating to the AI assistant.</summary>
public sealed class AskCopilotQueryHandler(
    IAiAssistantService ai,
    ILogger<AskCopilotQueryHandler> logger)
    : IRequestHandler<AskCopilotQuery, Result<CopilotAnswer>>
{
    /// <inheritdoc/>
    public async Task<Result<CopilotAnswer>> Handle(AskCopilotQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return Result<CopilotAnswer>.Failure(new Error("Copilot.EmptyQuestion", "Question cannot be empty."));

        try
        {
            var result = await ai.AskCopilotAsync(request.Question, request.Tasks, request.ConversationHistory, ct);
            return Result<CopilotAnswer>.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI copilot failed: service is misconfigured.");
            return Result<CopilotAnswer>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI copilot failed due to an unexpected error.");
            return Result<CopilotAnswer>.Failure(AiErrors.Unavailable);
        }
    }
}
