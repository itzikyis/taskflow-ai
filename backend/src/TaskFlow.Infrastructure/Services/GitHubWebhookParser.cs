using System.Text.Json;
using TaskFlow.Application.DevelopmentLinks.GitHub;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Services;

/// <summary>Parses GitHub "push" and "pull_request" webhook payloads.</summary>
public sealed class GitHubWebhookParser : IGitHubWebhookParser
{
    /// <inheritdoc/>
    public IReadOnlyList<ParsedDevelopmentRef> Parse(string eventType, string jsonPayload)
    {
        if (string.IsNullOrWhiteSpace(jsonPayload))
            return [];

        using var doc = JsonDocument.Parse(jsonPayload);
        var root = doc.RootElement;

        return eventType switch
        {
            "push"         => ParsePush(root),
            "pull_request" => ParsePullRequest(root),
            _              => [],
        };
    }

    private static List<ParsedDevelopmentRef> ParsePush(JsonElement root)
    {
        var results = new List<ParsedDevelopmentRef>();

        var repository = GetString(root, "repository", "full_name") ?? "unknown/unknown";
        var branch = TryGetString(root, "ref")?.Replace("refs/heads/", string.Empty) ?? string.Empty;

        if (!root.TryGetProperty("commits", out var commits) || commits.ValueKind != JsonValueKind.Array)
            return results;

        foreach (var commit in commits.EnumerateArray())
        {
            var message = TryGetString(commit, "message") ?? string.Empty;
            var sha = TryGetString(commit, "id");
            var url = TryGetString(commit, "url") ?? string.Empty;
            var title = message.Split('\n', 2)[0].Trim();

            results.Add(new ParsedDevelopmentRef(
                Repository: repository,
                RefType: DevelopmentRefType.Commit,
                Title: title.Length == 0 ? (sha ?? "commit") : title,
                Url: url,
                Status: DevelopmentLinkStatus.None,
                ExternalId: sha,
                // Scan both the commit message and the branch name for task refs.
                TextToScan: $"{branch}\n{message}"));
        }

        return results;
    }

    private static List<ParsedDevelopmentRef> ParsePullRequest(JsonElement root)
    {
        if (!root.TryGetProperty("pull_request", out var pr))
            return [];

        var repository = GetString(root, "repository", "full_name") ?? "unknown/unknown";
        var title = TryGetString(pr, "title") ?? "Pull request";
        var body = TryGetString(pr, "body") ?? string.Empty;
        var url = TryGetString(pr, "html_url") ?? string.Empty;
        var branch = GetString(pr, "head", "ref") ?? string.Empty;
        var number = pr.TryGetProperty("number", out var n) && n.ValueKind == JsonValueKind.Number
            ? n.GetInt64().ToString()
            : null;

        var state = TryGetString(pr, "state");
        var merged = pr.TryGetProperty("merged", out var m) && m.ValueKind == JsonValueKind.True;
        var draft = pr.TryGetProperty("draft", out var d) && d.ValueKind == JsonValueKind.True;

        var status = merged
            ? DevelopmentLinkStatus.Merged
            : state == "closed"
                ? DevelopmentLinkStatus.Closed
                : draft
                    ? DevelopmentLinkStatus.Draft
                    : DevelopmentLinkStatus.Open;

        return
        [
            new ParsedDevelopmentRef(
                Repository: repository,
                RefType: DevelopmentRefType.PullRequest,
                Title: title,
                Url: url,
                Status: status,
                ExternalId: number,
                TextToScan: $"{branch}\n{title}\n{body}")
        ];
    }

    private static string? TryGetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? GetString(JsonElement element, string parent, string child) =>
        element.TryGetProperty(parent, out var p) && p.ValueKind == JsonValueKind.Object
            ? TryGetString(p, child)
            : null;
}
