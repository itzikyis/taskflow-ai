# MCP Integration — TaskFlow AI

TaskFlow AI exposes a lightweight [Model Context Protocol](https://modelcontextprotocol.io/) (MCP)
surface so external AI assistants (Claude Desktop, GitHub Copilot, etc.) can list, create, and
search tasks and projects without leaving the assistant UI.

---

## Endpoints

| Method | Path | Auth required | Purpose |
|--------|------|---------------|---------|
| `GET`  | `/api/mcp/tools` | No | Discover available tools (capability advertisement) |
| `POST` | `/api/mcp/call`  | Yes (Bearer) | Invoke a tool |

Base URL (local development): `http://localhost:5000`

---

## Available tools

| Tool name | Description | Required parameters |
|-----------|-------------|---------------------|
| `list_tasks` | List all tasks | — |
| `create_task` | Create a new task | `title` |
| `get_task` | Get a task by ID | `id` |
| `list_projects` | List all projects | — |
| `search_tasks` | Natural-language task search | `query` |

### Tool parameter details

#### `create_task`
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| `title` | string | Yes | Task title |
| `description` | string | No | Longer description |
| `priority` | string | No | `Low`, `Medium` (default), `High`, or `Critical` |
| `dueDate` | string | No | ISO-8601 date, e.g. `2025-12-31` |

#### `get_task`
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| `id` | string | Yes | Task GUID |

#### `search_tasks`
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| `query` | string | Yes | Free-text, e.g. `"overdue high-priority tasks assigned to me"` |

---

## Authentication

`POST /api/mcp/call` requires a valid JWT issued by the TaskFlow API.

1. Obtain a token via `POST /api/auth/login`.
2. Pass it as a Bearer token on every call endpoint request:

```
Authorization: Bearer <your-token>
```

The tools endpoint (`GET /api/mcp/tools`) is public and requires no token.

---

## Configuring Claude Desktop

Add the following to your `claude_desktop_config.json`
(`~/Library/Application Support/Claude/` on macOS,
`%APPDATA%\Claude\` on Windows):

```json
{
  "mcpServers": {
    "taskflow": {
      "type": "http",
      "baseUrl": "http://localhost:5000/api/mcp",
      "headers": {
        "Authorization": "Bearer <your-token>"
      }
    }
  }
}
```

Replace `<your-token>` with a token obtained from `POST /api/auth/login`.

---

## Example tool calls

### List all tasks

```json
POST /api/mcp/call
Content-Type: application/json
Authorization: Bearer <token>

{
  "tool": "list_tasks",
  "parameters": {}
}
```

Response:
```json
{
  "result": [
    { "id": "3fa85f64-...", "title": "Design new dashboard", "priority": "High", ... }
  ]
}
```

### Create a task

```json
POST /api/mcp/call
Content-Type: application/json
Authorization: Bearer <token>

{
  "tool": "create_task",
  "parameters": {
    "title": "Write unit tests for auth module",
    "priority": "High",
    "dueDate": "2025-09-30"
  }
}
```

Response:
```json
{
  "result": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Search tasks

```json
POST /api/mcp/call
Content-Type: application/json
Authorization: Bearer <token>

{
  "tool": "search_tasks",
  "parameters": {
    "query": "overdue high-priority tasks"
  }
}
```

Response:
```json
{
  "result": {
    "interpretation": "Tasks with High or Critical priority that are past their due date",
    "results": [ ... ]
  }
}
```

---

## Security notes

- Tokens are short-lived JWTs; refresh them via `POST /api/auth/refresh`.
- The MCP endpoints respect the same authorization rules as the rest of the API — a token
  only grants access to resources the authenticated user can see.
- Do **not** expose `http://localhost:5000` to the internet without TLS termination.
