using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Integrations.Slack.SaveSlackConfig;

/// <summary>Handles <see cref="SaveSlackConfigCommand"/> (upsert of the single config row).</summary>
public sealed class SaveSlackConfigCommandHandler(ISlackIntegrationRepository repo)
    : IRequestHandler<SaveSlackConfigCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(SaveSlackConfigCommand request, CancellationToken ct)
    {
        var existing = await repo.GetAsync(ct);
        if (existing is null)
        {
            var created = SlackIntegration.Create(request.WebhookUrl);
            created.Update(request.WebhookUrl, request.Enabled, request.ForwardCreated,
                request.ForwardStatusChanged, request.ForwardComments, request.ForwardOther);
            await repo.AddAsync(created, ct);
        }
        else
        {
            existing.Update(request.WebhookUrl, request.Enabled, request.ForwardCreated,
                request.ForwardStatusChanged, request.ForwardComments, request.ForwardOther);
            repo.Update(existing);
        }

        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
