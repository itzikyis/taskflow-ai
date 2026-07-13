namespace TaskFlow.Application.Interfaces;

/// <summary>Sends a plain-text notification to an external chat system (e.g. Slack).</summary>
public interface IExternalNotificationService
{
    /// <summary>Posts <paramref name="text"/> to the given webhook URL. Best-effort; may throw on transport errors.</summary>
    Task SendAsync(string webhookUrl, string text, CancellationToken ct = default);
}
