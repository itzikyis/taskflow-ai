using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TaskTemplates.Commands.DeleteTaskTemplate;

/// <summary>Handles <see cref="DeleteTaskTemplateCommand"/>.</summary>
public sealed class DeleteTaskTemplateCommandHandler(ITaskTemplateRepository repository)
    : IRequestHandler<DeleteTaskTemplateCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        DeleteTaskTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = await repository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
            return Result.Failure(TaskTemplateErrors.NotFound);

        repository.Delete(template);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}
