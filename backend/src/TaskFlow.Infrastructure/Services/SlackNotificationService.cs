using System.Text;
using System.Text.Json;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>Posts messages to a Slack Incoming Webhook URL.</summary>
public sealed class SlackNotificationService(HttpClient http) : IExternalNotificationService
{
    /// <inheritdoc/>
    public async Task SendAsync(string webhookUrl, string text, CancellationToken ct = default)
    {
        var body = JsonSerializer.Serialize(new { text });
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await http.PostAsync(webhookUrl, content, ct);
        response.EnsureSuccessStatusCode();
    }
}
