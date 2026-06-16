# Issue #6 — Implement AI Assistant Module

## Overview
An **AI Assistant** powered by Anthropic Claude helps users with task management: suggesting due dates, breaking tasks into subtasks, summarising comments, and answering questions about their project. All AI calls go through a dedicated Application service — never directly from controllers.

## Acceptance Criteria

### Domain
- [ ] `AiSuggestion` value object: `{ Prompt, Response, ModelId, CreatedAt }` (for audit log)
- [ ] `AiErrors` constants: `ServiceUnavailable`, `ContextTooLarge`, `EmptyPrompt`

### Application
- [ ] `IAiAssistantService` in `Application/Interfaces/`:
  ```csharp
  public interface IAiAssistantService
  {
      Task<Result<string>> SuggestDueDateAsync(string taskTitle, string? description, CancellationToken ct);
      Task<Result<IReadOnlyList<string>>> SuggestSubtasksAsync(string taskTitle, string? description, CancellationToken ct);
      Task<Result<string>> SummariseCommentsAsync(IReadOnlyList<string> commentBodies, CancellationToken ct);
      Task<Result<string>> AskAsync(string userQuestion, string projectContext, CancellationToken ct);
  }
  ```
- [ ] Commands / Queries:
  - `SuggestDueDateQuery` (taskId) → `Result<string>` (ISO date string)
  - `SuggestSubtasksQuery` (taskId) → `Result<IReadOnlyList<string>>`
  - `SummariseTaskCommentsQuery` (taskId) → `Result<string>`
  - `AskAiQuery` (projectId, question) → `Result<string>`
- [ ] Each handler fetches context (task title, description, comments) from repositories, then calls `IAiAssistantService`

### Infrastructure
- [ ] `ClaudeAiAssistantService` implementing `IAiAssistantService`:
  - Uses `Anthropic.SDK` NuGet package (or `HttpClient` directly to `https://api.anthropic.com/v1/messages`)
  - Model: `claude-sonnet-4-6` (configurable via `Ai:ModelId` config key)
  - System prompt establishes TaskFlow context and returns structured responses
  - API key from `Ai:AnthropicApiKey` config (never hard-coded)
  - Handles `429 Too Many Requests` → returns `AiErrors.ServiceUnavailable`
- [ ] Register in DI — use `AddHttpClient<ClaudeAiAssistantService>` for proper lifecycle

### API
- [ ] `AiController`:
  - `GET /api/tasks/{taskId}/ai/suggest-due-date`
  - `GET /api/tasks/{taskId}/ai/suggest-subtasks`
  - `GET /api/tasks/{taskId}/ai/summarise-comments`
  - `POST /api/projects/{projectId}/ai/ask` (body: `{ question: string }`)
- [ ] All endpoints require `[Authorize]`
- [ ] Rate-limit by user (to avoid abuse) — middleware or per-action check

### Frontend
- [ ] `src/features/ai/types/ai.types.ts`
- [ ] `src/services/aiService.ts`
- [ ] `src/features/ai/hooks/useAi.ts`
- [ ] `AiSuggestionButton` — inline button on task cards/forms that triggers a query and shows the result in a popover
- [ ] `AiDueDateSuggestion` — shows suggested date with "Apply" button that pre-fills the due date field
- [ ] `AiSubtaskSuggestions` — shows checklist of suggested subtasks, each with "Add as task" button
- [ ] `AiCommentSummary` — "Summarise comments" button in task detail view
- [ ] `AiChatPanel` — floating side panel per project for free-text Q&A
- [ ] Loading skeleton while AI responds; error state with retry

### Configuration

```json
// appsettings.json
"Ai": {
  "AnthropicApiKey": "",
  "ModelId": "claude-sonnet-4-6",
  "MaxTokens": 1024
}
```

```yaml
# docker-compose.yml — add to backend environment
Ai__AnthropicApiKey: "${ANTHROPIC_API_KEY}"
Ai__ModelId: "claude-sonnet-4-6"
```

### Tests
- [ ] `SuggestDueDateQueryHandlerTests` — mocks `IAiAssistantService`, verifies task context is fetched and passed
- [ ] `ClaudeAiAssistantServiceTests` (integration, optional) — calls real API only in CI with secret

## Infrastructure csproj additions

```xml
<PackageReference Include="Anthropic.SDK" Version="3.*" />
```

## Labels
`feature`, `backend`, `frontend`, `ai`
