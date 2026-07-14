using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.ProjectDocs.Commands.CreateProjectDocument;

/// <summary>Handles <see cref="CreateProjectDocumentCommand"/>.</summary>
public sealed class CreateProjectDocumentCommandHandler(IProjectDocumentRepository repo)
    : IRequestHandler<CreateProjectDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProjectDocumentCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<Guid>.Failure(new Error("ProjectDoc.InvalidTitle", "Title cannot be empty."));

        var doc = ProjectDocument.Create(request.ProjectId, request.Title, request.Body, request.AuthorId);
        await repo.AddAsync(doc, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(doc.Id);
    }
}
