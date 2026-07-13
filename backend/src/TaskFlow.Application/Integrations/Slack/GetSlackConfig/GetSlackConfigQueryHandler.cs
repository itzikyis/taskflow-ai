using MediatR;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Integrations.Slack.GetSlackConfig;

/// <summary>Handles <see cref="GetSlackConfigQuery"/>.</summary>
public sealed class GetSlackConfigQueryHandler(ISlackIntegrationRepository repo)
    : IRequestHandler<GetSlackConfigQuery, SlackConfigDto>
{
    /// <inheritdoc/>
    public async Task<SlackConfigDto> Handle(GetSlackConfigQuery request, CancellationToken ct)
    {
        var s = await repo.GetAsync(ct);
        if (s is null)
            return new SlackConfigDto(false, false, string.Empty, true, true, true, false);

        return new SlackConfigDto(
            true, s.Enabled, Mask(s.WebhookUrl),
            s.ForwardCreated, s.ForwardStatusChanged, s.ForwardComments, s.ForwardOther);
    }

    private static string Mask(string url) =>
        url.Length <= 12 ? "••••" : $"{url[..24]}…{url[^4..]}";
}
