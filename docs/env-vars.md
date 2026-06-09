# Environment Variables

## Backend (TaskFlow.API)

| Variable | Required | Default | Description |
|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | ✅ | — | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | ✅ | `Production` | `Development` / `Staging` / `Production` |
| `Jwt__Secret` | ✅ | — | Secret key for JWT signing (min 32 chars) |
| `Jwt__Issuer` | ✅ | — | JWT issuer claim |
| `Jwt__Audience` | ✅ | — | JWT audience claim |
| `Jwt__ExpiryMinutes` | ❌ | `60` | JWT token TTL in minutes |

## Frontend

| Variable | Required | Default | Description |
|---|---|---|---|
| `VITE_API_BASE_URL` | ❌ | `/api` | Backend base URL (used in production builds) |

## Local development

Create a `.env.local` file in `frontend/` (never commit):

```
VITE_API_BASE_URL=http://localhost:5000/api
```

For backend secrets in development, use **user-secrets**:

```bash
cd backend/src/TaskFlow.API
dotnet user-secrets set "Jwt:Secret" "your-very-long-dev-secret-here"
```
