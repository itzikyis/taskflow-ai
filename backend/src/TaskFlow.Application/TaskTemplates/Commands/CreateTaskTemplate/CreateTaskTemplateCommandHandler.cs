using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.TaskTemplates.Commands.CreateTaskTemplate;

/// <summary>Handles <see cref="CreateTaskTemplateCommand"/>.</summary>
public sealed class CreateTaskTemplateCommandHandler(ITaskTemplateRepository repository)
    : IRequestHandler<CreateTaskTemplateCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        CreateTaskTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var result = TaskTemplate.Create(
            request.ProjectId,
            request.Name,
            request.DefaultTitle,
            request.DefaultDescription,
            request.DefaultPriority,
            request.DefaultEstimatedHours);

        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        await repository.AddAsync(result.Value!, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result.Value!.Id);
    }
}
