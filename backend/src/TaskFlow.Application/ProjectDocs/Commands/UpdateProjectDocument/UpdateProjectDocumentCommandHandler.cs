using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ProjectDocs.Commands.UpdateProjectDocument;

/// <summary>Handles <see cref="UpdateProjectDocumentCommand"/>.</summary>
public sealed class UpdateProjectDocumentCommandHandler(IProjectDocumentRepository repo)
    : IRequestHandler<UpdateProjectDocumentCommand, Result>
{
    public async Task<Result> Handle(UpdateProjectDocumentCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure(new Error("ProjectDoc.InvalidTitle", "Title cannot be empty."));

        var doc = await repo.GetByIdAsync(request.Id, ct);
        if (doc is null)
            return Result.Failure(new Error("ProjectDoc.NotFound", $"Document {request.Id} not found."));

        doc.Update(request.Title, request.Body);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
