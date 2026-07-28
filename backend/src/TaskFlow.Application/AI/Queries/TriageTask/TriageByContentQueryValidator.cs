using FluentValidation;

namespace TaskFlow.Application.AI.Queries.TriageTask;

/// <summary>Validates <see cref="TriageByContentQuery"/> before the handler runs.</summary>
public sealed class TriageByContentQueryValidator : AbstractValidator<TriageByContentQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public TriageByContentQueryValidator()
    {
        RuleFor(q => q.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(q => q.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
