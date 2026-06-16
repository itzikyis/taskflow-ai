using MediatR;
using TaskFlow.Application.Attachments.Dtos;

namespace TaskFlow.Application.Attachments.Queries.GetAttachmentsByTask;

/// <summary>Returns all attachments for a task, ordered by upload time descending.</summary>
public sealed record GetAttachmentsByTaskQuery(Guid TaskId) : IRequest<IReadOnlyList<AttachmentDto>>;
