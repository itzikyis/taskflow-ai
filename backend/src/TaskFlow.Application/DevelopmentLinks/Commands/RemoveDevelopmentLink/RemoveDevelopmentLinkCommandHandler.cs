using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.DevelopmentLinks.Commands.RemoveDevelopmentLink;

/// <summary>Handles <see cref="RemoveDevelopmentLinkCommand"/>.</summary>
public sealed class RemoveDevelopmentLinkCommandHandler(IDevelopmentLinkRepository repo)
    : IRequestHandler<RemoveDevelopmentLinkCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RemoveDevelopmentLinkCommand request, CancellationToken ct)
    {
        var link = await repo.GetByIdAsync(request.LinkId, ct);
        if (link is null)
            return Result.Failure(DevelopmentLinkErrors.NotFound);

        repo.Remove(link);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
