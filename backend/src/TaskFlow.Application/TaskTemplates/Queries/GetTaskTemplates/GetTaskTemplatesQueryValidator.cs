using FluentValidation;

namespace TaskFlow.Application.TaskTemplates.Queries.GetTaskTemplates;

/// <summary>Validates <see cref="GetTaskTemplatesQuery"/> inputs.</summary>
public sealed class GetTaskTemplatesQueryValidator : AbstractValidator<GetTaskTemplatesQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetTaskTemplatesQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
