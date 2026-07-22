using FluentValidation;

namespace TaskFlow.Application.AI.Queries.TriageTask;

/// <summary>Validates <see cref="TriageTaskQuery"/> before the AI call is made.</summary>
public sealed class TriageTaskQueryValidator : AbstractValidator<TriageTaskQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public TriageTaskQueryValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId must not be empty.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId must not be empty.");
    }
}
