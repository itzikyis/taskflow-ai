using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Attachments.Commands.DeleteAttachment;

/// <summary>Handles <see cref="DeleteAttachmentCommand"/>.</summary>
public sealed class DeleteAttachmentCommandHandler(IAttachmentRepository repo)
    : IRequestHandler<DeleteAttachmentCommand, Result>
{
    public async Task<Result> Handle(DeleteAttachmentCommand request, CancellationToken ct)
    {
        var attachment = await repo.GetByIdAsync(request.AttachmentId, ct);
        if (attachment is null) return Result.Failure(AttachmentErrors.NotFound);
        if (attachment.UploadedBy != request.RequesterId) return Result.Failure(AttachmentErrors.NotOwner);
        repo.Remove(attachment);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
