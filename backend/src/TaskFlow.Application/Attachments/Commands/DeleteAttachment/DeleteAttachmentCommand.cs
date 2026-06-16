using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Attachments.Commands.DeleteAttachment;

/// <summary>Deletes an attachment (file must be removed from storage by the caller).</summary>
public sealed record DeleteAttachmentCommand(Guid AttachmentId, Guid RequesterId) : IRequest<Result>;
