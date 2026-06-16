using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>AI assistant powered by the Anthropic Claude API.</summary>
public sealed class ClaudeAiAssistantService : IAiAssistantService
{
    private readonly HttpClient _http;
    private readonly string _model;

    public ClaudeAiAssistantService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        var apiKey = configuration["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic API key not configured. Set the Anthropic__ApiKey environment variable.");
        _model = configuration["Anthropic:Model"] ?? "claude-haiku-4-5-20251001";
        _http.BaseAddress = new Uri("https://api.anthropic.com");
        _http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public Task<string> SuggestTaskDescriptionAsync(string taskTitle, CancellationToken ct) =>
        CallClaudeAsync(
            $"You are a helpful project management assistant. " +
            $"Given the task title \"{taskTitle}\", write a clear, concise task description (2-3 sentences) " +
            "that explains what needs to be done. Reply with only the description text, no preamble.",
            ct);

    public Task<string> SuggestDueDateAsync(string taskTitle, string? taskDescription, CancellationToken ct)
    {
        var context = string.IsNullOrWhiteSpace(taskDescription)
            ? $"Task: \"{taskTitle}\""
            : $"Task: \"{taskTitle}\"\nDescription: {taskDescription}";
        return CallClaudeAsync(
            $"You are a project management assistant. Today is {DateTime.UtcNow:yyyy-MM-dd}. " +
            $"Based on the following task, suggest a realistic due date and explain briefly why:\n\n{context}\n\n" +
            "Reply in the format: DATE: YYYY-MM-DD\nREASONING: one sentence.",
            ct);
    }

    public Task<string> SummarizeCommentsAsync(IEnumerable<string> comments, CancellationToken ct)
    {
        var joined = string.Join("\n---\n", comments.Select((c, i) => $"Comment {i + 1}: {c}"));
        return CallClaudeAsync(
            "You are a project management assistant. Summarize the following task comments into one concise paragraph " +
            "highlighting the key decisions, blockers, and action items:\n\n" + joined +
            "\n\nReply with only the summary paragraph.",
            ct);
    }

    private async Task<string> CallClaudeAsync(string prompt, CancellationToken ct)
    {
        var body = JsonSerializer.Serialize(new
        {
            model = _model,
            max_tokens = 512,
            messages = new[] { new { role = "user", content = prompt } }
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement
                  .GetProperty("content")[0]
                  .GetProperty("text")
                  .GetString() ?? string.Empty;
    }
}
