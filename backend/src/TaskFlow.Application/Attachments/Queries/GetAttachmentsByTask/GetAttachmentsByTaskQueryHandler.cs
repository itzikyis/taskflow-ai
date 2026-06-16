using MediatR;
using TaskFlow.Application.Attachments.Dtos;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Attachments.Queries.GetAttachmentsByTask;

/// <summary>Handles <see cref="GetAttachmentsByTaskQuery"/>.</summary>
public sealed class GetAttachmentsByTaskQueryHandler(IAttachmentRepository repo)
    : IRequestHandler<GetAttachmentsByTaskQuery, IReadOnlyList<AttachmentDto>>
{
    public async Task<IReadOnlyList<AttachmentDto>> Handle(GetAttachmentsByTaskQuery request, CancellationToken ct)
    {
        var attachments = await repo.GetByTaskIdAsync(request.TaskId, ct);
        return attachments
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentDto(
                a.Id, a.TaskId, a.UploadedBy, a.FileName,
                a.ContentType, a.FileSizeBytes, a.StorageUrl, a.UploadedAt))
            .ToList();
    }
}
