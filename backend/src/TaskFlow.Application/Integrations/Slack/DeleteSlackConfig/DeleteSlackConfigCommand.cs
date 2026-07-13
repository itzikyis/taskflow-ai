using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Integrations.Slack.DeleteSlackConfig;

/// <summary>Removes the Slack integration config.</summary>
public sealed record DeleteSlackConfigCommand : IRequest<Result>;

/// <summary>Handles <see cref="DeleteSlackConfigCommand"/>.</summary>
public sealed class DeleteSlackConfigCommandHandler(ISlackIntegrationRepository repo)
    : IRequestHandler<DeleteSlackConfigCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteSlackConfigCommand request, CancellationToken ct)
    {
        var slack = await repo.GetAsync(ct);
        if (slack is not null)
        {
            repo.Remove(slack);
            await repo.SaveChangesAsync(ct);
        }
        return Result.Ok;
    }
}
