using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Queries.AssessSprintRisk;
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

    public Task<string> DraftTaskApproachAsync(string title, string? description, CancellationToken ct)
    {
        var context = string.IsNullOrWhiteSpace(description)
            ? $"Task: \"{title}\""
            : $"Task: \"{title}\"\nDescription: {description}";
        return CallClaudeAsync(
            "You are an AI agent that has just been assigned this task. Write a short first-pass " +
            "\"proposed approach\" comment (3-5 concise bullet points) outlining how you'd tackle it and " +
            "what you'd need from the team. Be practical. Reply with only the comment text.\n\n" + context,
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

    /// <inheritdoc/>
    public async Task<StoryPointEstimate> EstimateStoryPointsAsync(string title, string? description, CancellationToken ct)
    {
        var context = string.IsNullOrWhiteSpace(description)
            ? $"Title: {title}"
            : $"Title: {title}\nDescription: {description}";

        var raw = await CallClaudeAsync(
            "You are an agile story point estimator using the Fibonacci scale (1, 2, 3, 5, 8, 13). " +
            "Estimate story points for the following task.\n\n" + context + "\n\n" +
            "Reply in EXACTLY this format:\nPOINTS: <number>\nREASONING: <one sentence>\n" +
            "Valid values for POINTS are only: 1, 2, 3, 5, 8, 13.", ct);

        var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var pointsLine = lines.FirstOrDefault(l => l.StartsWith("POINTS:"));
        var reasoningLine = lines.FirstOrDefault(l => l.StartsWith("REASONING:"));

        var validPoints = new[] { 1, 2, 3, 5, 8, 13 };
        var points = 3; // default
        if (pointsLine is not null
            && int.TryParse(pointsLine.Replace("POINTS:", "").Trim(), out var parsed)
            && validPoints.Contains(parsed))
        {
            points = parsed;
        }

        var reasoning = reasoningLine?.Replace("REASONING:", "").Trim() ?? "Unable to parse reasoning.";
        return new StoryPointEstimate(points, reasoning);
    }

    /// <inheritdoc/>
    public async Task<SprintPlan> SuggestSprintPlanAsync(
        IEnumerable<(Guid Id, string Title, string? Description, string Priority, string Status)> backlog,
        int sprintCapacity,
        CancellationToken ct)
    {
        var backlogList = backlog.ToList();
        var backlogSummary = string.Join("\n", backlogList.Select(t =>
            $"- [{t.Priority}] (id:{t.Id}) {t.Title}: {t.Description ?? "No description"}"));

        var prompt =
            $"You are an agile sprint planning assistant. Given a backlog of tasks and a team capacity of {sprintCapacity} story points, suggest an optimal sprint plan.\n\n" +
            $"Backlog:\n{backlogSummary}\n\n" +
            "Reply in EXACTLY this JSON format (no markdown, no extra text):\n" +
            "{\n" +
            "  \"sprintGoal\": \"...\",\n" +
            "  \"suggestedTasks\": [\n" +
            "    { \"taskId\": \"...\", \"title\": \"...\", \"estimatedPoints\": 3, \"justification\": \"...\" }\n" +
            "  ],\n" +
            "  \"reasoning\": \"...\"\n" +
            "}\n\n" +
            $"Choose tasks that: (1) have high priority, (2) fit within {sprintCapacity} total story points, (3) form a coherent sprint goal. Valid point values: 1,2,3,5,8,13.";

        var raw = await CallClaudeAsync(prompt, ct, maxTokens: 1024);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<SprintPlanResponse>(raw, options);
            if (response is null) return FallbackPlan();

            var tasks = (response.SuggestedTasks ?? []).Select(t => new SprintTaskSuggestion(
                Guid.TryParse(t.TaskId, out var guid) ? guid : Guid.Empty,
                t.Title ?? string.Empty,
                t.EstimatedPoints,
                t.Justification ?? string.Empty)).ToList();

            return new SprintPlan(
                response.SprintGoal ?? "No goal specified",
                tasks,
                response.Reasoning ?? string.Empty);
        }
        catch
        {
            return FallbackPlan();
        }
    }

    /// <inheritdoc/>
    public async Task<ReleaseNotes> GenerateReleaseNotesAsync(
        string version,
        IEnumerable<(string Title, string? Description, string Priority)> completedTasks,
        CancellationToken ct)
    {
        var taskList = completedTasks.ToList();
        var tasksSummary = string.Join("\n", taskList.Select(t =>
            $"- [{t.Priority}] {t.Title}: {t.Description ?? "No description"}"));

        var prompt =
            $"You are a technical writer generating release notes for version {version}.\n" +
            "Based on the following completed tasks, generate professional release notes.\n\n" +
            $"Completed Tasks:\n{tasksSummary}\n\n" +
            "Reply in EXACTLY this JSON format (no markdown code blocks, raw JSON only):\n" +
            "{\n" +
            $"  \"version\": \"{version}\",\n" +
            "  \"summary\": \"A 1-2 sentence overview of this release\",\n" +
            "  \"features\": [\"Feature 1 description\", \"Feature 2 description\"],\n" +
            "  \"bugFixes\": [\"Bug fix 1\", \"Bug fix 2\"],\n" +
            "  \"improvements\": [\"Improvement 1\"],\n" +
            $"  \"markdownContent\": \"# Release Notes v{version}\\n\\n## Summary\\n...\\n\\n## New Features\\n- ...\\n\\n## Bug Fixes\\n- ...\"\n" +
            "}\n\n" +
            "Categorize each task as a feature, bug fix, or improvement based on context.\n" +
            "If there are no items in a category, use an empty array.";

        var raw = await CallClaudeAsync(prompt, ct, maxTokens: 1024);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<ReleaseNotesResponse>(raw, options);
            if (response is null) return FallbackReleaseNotes(version);

            return new ReleaseNotes(
                response.Version ?? version,
                response.Summary ?? string.Empty,
                response.Features ?? [],
                response.BugFixes ?? [],
                response.Improvements ?? [],
                response.MarkdownContent ?? string.Empty);
        }
        catch
        {
            return FallbackReleaseNotes(version);
        }
    }

    private static ReleaseNotes FallbackReleaseNotes(string version) =>
        new(version, "Unable to generate release notes.", [], [], [], "AI service returned an unparseable response.");

    private sealed record ReleaseNotesResponse(
        string? Version,
        string? Summary,
        List<string>? Features,
        List<string>? BugFixes,
        List<string>? Improvements,
        string? MarkdownContent);

    private static SprintPlan FallbackPlan() =>
        new("Unable to generate plan", [], "AI service returned an unparseable response.");

    private sealed record SprintPlanResponse(
        string? SprintGoal,
        List<SprintTaskSuggestionResponse>? SuggestedTasks,
        string? Reasoning);

    private sealed record SprintTaskSuggestionResponse(
        string? TaskId,
        string? Title,
        int EstimatedPoints,
        string? Justification);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SubtaskSuggestion>> GenerateSubtasksAsync(
        string title, string? description, CancellationToken ct)
    {
        var context = string.IsNullOrWhiteSpace(description)
            ? $"Title: {title}"
            : $"Title: {title}\nDescription: {description}";

        var prompt =
            "You are an agile delivery assistant. Break the following task down into a set of " +
            "3-8 concrete, actionable subtasks.\n\n" + context + "\n\n" +
            "Reply in EXACTLY this JSON format (no markdown, raw JSON array only):\n" +
            "[\n" +
            "  { \"title\": \"Short imperative subtask title\", \"description\": \"One sentence of detail\" }\n" +
            "]\n\n" +
            "Return between 3 and 8 items. Keep titles under 100 characters.";

        var raw = await CallClaudeAsync(prompt, ct, maxTokens: 1024);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<List<SubtaskResponse>>(raw, options);
            if (response is null || response.Count == 0)
                return [];

            return response
                .Where(s => !string.IsNullOrWhiteSpace(s.Title))
                .Select(s => new SubtaskSuggestion(s.Title!.Trim(), s.Description?.Trim()))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private sealed record SubtaskResponse(string? Title, string? Description);

    /// <inheritdoc/>
    public async Task<SprintRetrospective> GenerateRetrospectiveAsync(
        IEnumerable<(string Title, string? Description, string Priority)> completed,
        IEnumerable<(string Title, string? Description, string Priority)> incomplete,
        CancellationToken ct)
    {
        var doneList = completed.ToList();
        var openList = incomplete.ToList();

        string Format(IEnumerable<(string Title, string? Description, string Priority)> items) =>
            string.Join("\n", items.Select(t => $"- [{t.Priority}] {t.Title}: {t.Description ?? "No description"}"));

        var prompt =
            "You are an agile coach writing a sprint retrospective. Based on the completed and " +
            "incomplete work below, produce a concise, data-backed retrospective draft.\n\n" +
            $"Completed ({doneList.Count}):\n{Format(doneList)}\n\n" +
            $"Incomplete ({openList.Count}):\n{Format(openList)}\n\n" +
            "Reply in EXACTLY this JSON format (no markdown, raw JSON only):\n" +
            "{\n" +
            "  \"summary\": \"1-2 sentence overview of the sprint\",\n" +
            "  \"wentWell\": [\"...\"],\n" +
            "  \"issues\": [\"...\"],\n" +
            "  \"estimateAccuracyNotes\": [\"observations on scope/velocity/estimate accuracy\"],\n" +
            "  \"actionItems\": [\"concrete improvements for next sprint\"]\n" +
            "}\n\n" +
            "Use empty arrays where there is nothing to report.";

        var raw = await CallClaudeAsync(prompt, ct, maxTokens: 1024);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<RetrospectiveResponse>(raw, options);
            if (response is null) return FallbackRetrospective();

            return new SprintRetrospective(
                response.Summary ?? string.Empty,
                response.WentWell ?? [],
                response.Issues ?? [],
                response.EstimateAccuracyNotes ?? [],
                response.ActionItems ?? []);
        }
        catch
        {
            return FallbackRetrospective();
        }
    }

    private static SprintRetrospective FallbackRetrospective() =>
        new("Unable to generate retrospective.", [], [], [], []);

    private sealed record RetrospectiveResponse(
        string? Summary,
        List<string>? WentWell,
        List<string>? Issues,
        List<string>? EstimateAccuracyNotes,
        List<string>? ActionItems);

    /// <inheritdoc/>
    public async Task<SprintRiskAssessment> AssessSprintRiskAsync(
        IReadOnlyList<RiskTaskInput> tasks,
        CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var taskSummary = string.Join("\n", tasks.Select(t =>
        {
            var daysSinceUpdate = t.UpdatedAt.HasValue
                ? (int)(today - t.UpdatedAt.Value.Date).TotalDays
                : (int)(today - t.CreatedAt.Date).TotalDays;
            var dueInfo = t.DueDate.HasValue
                ? $"due {t.DueDate.Value:yyyy-MM-dd} ({(int)(t.DueDate.Value.Date - today).TotalDays}d left)"
                : "no due date";
            return $"- id:{t.Id} | [{t.Priority}] [{t.Status}] {t.Title} | {dueInfo} | last activity {daysSinceUpdate}d ago | blockers:{t.OpenBlockerCount}";
        }));

        var prompt =
            $"You are a senior project manager assessing risk for tasks in an active sprint. Today is {today:yyyy-MM-dd}.\n\n" +
            $"Tasks:\n{taskSummary}\n\n" +
            "Score each task as OnTrack, AtRisk, or Blocked based on:\n" +
            "- Overdue or <3 days to deadline with non-Done status → AtRisk or Blocked\n" +
            "- Stalled (no activity >5 days, not Done) → AtRisk\n" +
            "- Has open blockers → Blocked\n" +
            "- Everything else → OnTrack\n\n" +
            "Reply in EXACTLY this JSON format (no markdown, raw JSON only):\n" +
            "{\n" +
            "  \"tasks\": [\n" +
            "    { \"taskId\": \"<guid>\", \"title\": \"...\", \"level\": \"OnTrack|AtRisk|Blocked\", \"reason\": \"one concise sentence\" }\n" +
            "  ],\n" +
            "  \"summary\": \"2-3 sentence sprint health summary\",\n" +
            "  \"recommendations\": [\"actionable recommendation 1\", \"actionable recommendation 2\"]\n" +
            "}";

        var raw = await CallClaudeAsync(prompt, ct, maxTokens: 1500);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<RiskAssessmentResponse>(raw, options);
            if (response is null) return FallbackRisk(tasks);

            var scored = (response.Tasks ?? []).Select(t =>
            {
                var level = Enum.TryParse<RiskLevel>(t.Level, true, out var l) ? l : RiskLevel.OnTrack;
                return new TaskRiskScore(
                    Guid.TryParse(t.TaskId, out var g) ? g : Guid.Empty,
                    t.Title ?? string.Empty,
                    level,
                    t.Reason ?? string.Empty);
            }).ToList();

            return new SprintRiskAssessment(
                scored,
                scored.Count(s => s.Level == RiskLevel.OnTrack),
                scored.Count(s => s.Level == RiskLevel.AtRisk),
                scored.Count(s => s.Level == RiskLevel.Blocked),
                response.Summary ?? string.Empty,
                response.Recommendations ?? []);
        }
        catch
        {
            return FallbackRisk(tasks);
        }
    }

    private static SprintRiskAssessment FallbackRisk(IReadOnlyList<RiskTaskInput> tasks) =>
        new([], 0, 0, 0, "Unable to generate risk assessment.", []);

    /// <inheritdoc/>
    public async Task<MeetingNotesResult> AnalyzeMeetingNotesAsync(
        string transcript,
        IReadOnlyList<string> participantNames,
        CancellationToken ct)
    {
        var participantList = participantNames.Count > 0
            ? string.Join(", ", participantNames)
            : "unknown";

        var prompt =
            $"You are a project management assistant analyzing meeting notes.\n" +
            $"Participants: {participantList}\n\n" +
            $"Meeting transcript/notes:\n{transcript}\n\n" +
            "Extract the following and reply in EXACTLY this JSON format (no markdown, raw JSON only):\n" +
            "{\n" +
            "  \"summary\": \"2-3 sentence overview of what was discussed and decided\",\n" +
            "  \"keyDecisions\": [\"Decision 1\", \"Decision 2\"],\n" +
            "  \"actionItems\": [\n" +
            "    {\n" +
            "      \"title\": \"Short imperative task title\",\n" +
            "      \"description\": \"One sentence detail of what needs to be done\",\n" +
            "      \"priority\": \"Low|Medium|High|Critical\",\n" +
            "      \"suggestedAssignee\": \"Name from participants or null\",\n" +
            "      \"suggestedDueDate\": \"YYYY-MM-DD or null\"\n" +
            "    }\n" +
            "  ]\n" +
            "}\n\n" +
            "Rules:\n" +
            "- Extract only concrete action items with a clear owner or next step\n" +
            "- Priority: Critical for blockers, High for sprint commitments, Medium for near-term, Low for backlog\n" +
            "- suggestedAssignee must be one of the participant names or null\n" +
            "- Use empty arrays if nothing was found";

        var raw = await CallClaudeAsync(prompt, ct, maxTokens: 1500);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<MeetingNotesResponse>(raw, options);
            if (response is null) return FallbackMeetingNotes();

            var items = (response.ActionItems ?? []).Select(a => new MeetingActionItem(
                a.Title ?? string.Empty,
                a.Description ?? string.Empty,
                a.Priority ?? "Medium",
                a.SuggestedAssignee,
                a.SuggestedDueDate)).ToList();

            return new MeetingNotesResult(
                response.Summary ?? string.Empty,
                response.KeyDecisions ?? [],
                items);
        }
        catch
        {
            return FallbackMeetingNotes();
        }
    }

    private static MeetingNotesResult FallbackMeetingNotes() =>
        new("Unable to analyze meeting notes.", [], []);

    private sealed record MeetingNotesResponse(
        string? Summary,
        List<string>? KeyDecisions,
        List<MeetingActionItemResponse>? ActionItems);

    private sealed record MeetingActionItemResponse(
        string? Title,
        string? Description,
        string? Priority,
        string? SuggestedAssignee,
        string? SuggestedDueDate);

    private sealed record RiskAssessmentResponse(
        List<RiskTaskScoreResponse>? Tasks,
        string? Summary,
        List<string>? Recommendations);

    private sealed record RiskTaskScoreResponse(
        string? TaskId,
        string? Title,
        string? Level,
        string? Reason);

    private async Task<string> CallClaudeAsync(string prompt, CancellationToken ct, int maxTokens = 512)
    {
        var body = JsonSerializer.Serialize(new
        {
            model = _model,
            max_tokens = maxTokens,
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
