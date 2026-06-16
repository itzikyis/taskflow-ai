using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Attachments.Commands.AddAttachment;

/// <summary>Handles <see cref="AddAttachmentCommand"/>.</summary>
public sealed class AddAttachmentCommandHandler(IAttachmentRepository repo)
    : IRequestHandler<AddAttachmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddAttachmentCommand request, CancellationToken ct)
    {
        var result = Attachment.Create(
            request.TaskId, request.UploadedBy, request.FileName,
            request.ContentType, request.FileSizeBytes, request.StorageUrl);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);
        await repo.AddAsync(result.Value!, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value!.Id);
    }
}
