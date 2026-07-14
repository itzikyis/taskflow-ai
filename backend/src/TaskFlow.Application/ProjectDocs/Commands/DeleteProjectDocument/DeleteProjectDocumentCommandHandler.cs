using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ProjectDocs.Commands.DeleteProjectDocument;

/// <summary>Handles <see cref="DeleteProjectDocumentCommand"/>.</summary>
public sealed class DeleteProjectDocumentCommandHandler(IProjectDocumentRepository repo)
    : IRequestHandler<DeleteProjectDocumentCommand, Result>
{
    public async Task<Result> Handle(DeleteProjectDocumentCommand request, CancellationToken ct)
    {
        var doc = await repo.GetByIdAsync(request.Id, ct);
        if (doc is null)
            return Result.Failure(new Error("ProjectDoc.NotFound", $"Document {request.Id} not found."));

        await repo.DeleteAsync(doc, ct);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
