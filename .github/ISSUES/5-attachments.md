# Issue #5 — Implement Attachments Module

## Overview
Users can upload **file attachments** to Tasks. Files are stored on disk (local volume in dev, S3-compatible object storage in production). The database stores only metadata — never the binary content.

## Acceptance Criteria

### Domain
- [ ] `Attachment` entity (child of `TaskItem` aggregate):
  - `Id`, `TaskId`, `UploadedById`, `FileName` (original name), `StorageKey` (internal path/key), `ContentType`, `SizeBytes`, `CreatedAt`
- [ ] `AttachmentErrors` constants: `FileRequired`, `FileTooLarge` (max 20 MB), `UnsupportedContentType`, `NotFound`, `NotUploader`
- [ ] `Attachment.Create(taskId, uploadedById, fileName, storageKey, contentType, sizeBytes)` returning `Result<Attachment>`
- [ ] `AttachmentUploadedEvent` domain event

### Application
- [ ] `IAttachmentRepository` in `Application/Interfaces/`
- [ ] `IFileStorageService` in `Application/Interfaces/`:
  ```csharp
  public interface IFileStorageService
  {
      Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct);
      Task<Stream> DownloadAsync(string storageKey, CancellationToken ct);
      Task DeleteAsync(string storageKey, CancellationToken ct);
  }
  ```
- [ ] Commands:
  - `UploadAttachmentCommand` (taskId, uploadedById, fileName, contentType, content: Stream) → `Result<Guid>`
  - `DeleteAttachmentCommand` (attachmentId, requestingUserId) → `Result`
- [ ] Queries:
  - `GetAttachmentsByTaskQuery` (taskId) → `IReadOnlyList<AttachmentDto>`
  - `GetAttachmentDownloadQuery` (attachmentId) → `Result<AttachmentDownloadDto>`
- [ ] `AttachmentDto`: `{ Id, TaskId, FileName, ContentType, SizeBytes, UploadedById, CreatedAt, DownloadUrl }`
- [ ] `AttachmentDownloadDto`: `{ Stream, FileName, ContentType }`

### Infrastructure
- [ ] `AttachmentRepository` + `AttachmentConfiguration` (table: `attachments`)
- [ ] `LocalFileStorageService` — stores files in `/app/uploads/{storageKey}` (dev)
- [ ] `storageKey` = `{taskId}/{guid}-{sanitisedFileName}` to avoid collisions
- [ ] Register in DI with a toggle for local vs S3 via config

### API
- [ ] `AttachmentsController`:
  - `GET    /api/tasks/{taskId}/attachments` — list metadata
  - `POST   /api/tasks/{taskId}/attachments` — multipart/form-data upload
  - `GET    /api/attachments/{id}/download` — returns file stream
  - `DELETE /api/attachments/{id}`
- [ ] Max upload size configured in `Program.cs` (20 MB)
- [ ] Content-Disposition header on download for correct filename

### Frontend
- [ ] `src/features/attachments/types/attachment.types.ts`
- [ ] `src/services/attachmentService.ts` — multipart upload with Axios, progress tracking
- [ ] `src/features/attachments/hooks/useAttachments.ts`
- [ ] `AttachmentList` — shows file name, size, type icon, download link, delete button
- [ ] `AttachmentUpload` — drag-and-drop zone + file picker, shows upload progress bar
- [ ] Visible in task detail view alongside comments

### Tests
- [ ] `AttachmentTests` — domain unit tests (Create, FileTooLarge, UnsupportedType)
- [ ] `UploadAttachmentCommandHandlerTests` — mocks `IFileStorageService`, verifies storageKey saved

## Schema

```sql
CREATE TABLE attachments (
    id UUID PRIMARY KEY,
    task_id UUID NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
    uploaded_by_id UUID NOT NULL REFERENCES users(id),
    file_name VARCHAR(255) NOT NULL,
    storage_key VARCHAR(500) NOT NULL UNIQUE,
    content_type VARCHAR(100) NOT NULL,
    size_bytes BIGINT NOT NULL,
    created_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_attachments_task_id ON attachments(task_id);
```

## Docker / Infrastructure

- [ ] Add `/app/uploads` volume mount in `docker-compose.yml` for local dev
- [ ] Add `uploads/` to `.gitignore`

## Labels
`feature`, `backend`, `frontend`, `infrastructure`
