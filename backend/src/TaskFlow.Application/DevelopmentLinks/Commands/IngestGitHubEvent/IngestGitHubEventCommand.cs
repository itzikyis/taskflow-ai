using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.DevelopmentLinks.Commands.IngestGitHubEvent;

/// <summary>
/// Command to ingest a raw GitHub webhook payload, detect task references, and
/// create/update development links accordingly.
/// </summary>
/// <param name="EventType">The value of the <c>X-GitHub-Event</c> header.</param>
/// <param name="JsonPayload">The raw JSON request body.</param>
public sealed record IngestGitHubEventCommand(string EventType, string JsonPayload)
    : IRequest<Result<int>>;
