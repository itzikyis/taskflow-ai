---
name: frontend-agent
description: Use this agent for all frontend React + TypeScript tasks — components, hooks, services, stores, types, and tests. Invoke when the task touches code under frontend/src/.
---

# Frontend Agent — TaskFlow AI

## Stack
- React 18, TypeScript (strict), Vite
- React Query (`@tanstack/react-query`) — server state
- Zustand (with `persist` middleware) — global client state
- Axios — HTTP client
- Vitest + React Testing Library — unit/component tests
- Playwright — E2E tests

## Folder structure

```
src/
  features/<feature>/
    components/     # React components
    hooks/          # React Query hooks
    types/          # TypeScript types/interfaces
  components/       # Shared UI primitives
  services/         # Typed Axios service modules
  store/            # Zustand stores
```

## TypeScript rules

- `"strict": true` — no `any`. Use `unknown` and narrow.
- Functional components only — no class components.
- Named exports only — no default exports except `App`.
- One component per file, named after the component.
- Co-locate styles, tests, and types with the component.

## API calls

All HTTP calls go through service modules in `src/services/` — never raw `fetch` or `axios` directly in components.

```typescript
// src/services/taskService.ts
export const taskService = {
  getAll: async (): Promise<Task[]> => { ... },
  create: async (payload: CreateTaskPayload): Promise<string> => { ... },
};
```

## Server state (React Query)

Hooks in `src/features/<feature>/hooks/use<Feature>.ts`:

```typescript
export function useTasks() {
  return useQuery({ queryKey: ['tasks'], queryFn: taskService.getAll });
}

export function useCreateTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: taskService.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tasks'] }),
  });
}
```

## Auth state (Zustand)

`src/store/authStore.ts` — persisted to `localStorage`. On login, attaches `Authorization: Bearer <token>` to `axios.defaults.headers.common`. On logout, removes it. On rehydration (`onRehydrateStorage`), re-attaches the header.

## Type constants

```typescript
// Use const arrays + derive types from them
export const TASK_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export type TaskPriority = (typeof TASK_PRIORITIES)[number];
```

## Auth gating

`App.tsx` checks `useAuthStore().isAuthenticated`. Unauthenticated users see `LoginPage` / `RegisterPage`. Authenticated users see the full app with a header showing the display name and a Sign out button.

## Styling

Inline styles with `React.CSSProperties` — no CSS files unless the component warrants it. No magic numbers — use constants.

## Testing rules

- Test behaviour, not implementation — query by role/label/text, not by class or test-id (unless unavoidable).
- Unit/component tests with Vitest + React Testing Library.
- E2E tests with Playwright for critical flows: login, register, create task, create project.

## File naming

| Thing | Convention |
|---|---|
| Component | `PascalCase.tsx` |
| Hook | `useCamelCase.ts` |
| Service | `camelCaseService.ts` |
| Store | `camelCaseStore.ts` |
| Types | `feature.types.ts` |
