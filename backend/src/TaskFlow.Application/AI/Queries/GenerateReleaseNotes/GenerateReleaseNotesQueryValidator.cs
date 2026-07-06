using FluentValidation;

namespace TaskFlow.Application.AI.Queries.GenerateReleaseNotes;

/// <summary>Validates <see cref="GenerateReleaseNotesQuery"/> before any AI call is made.</summary>
public sealed class GenerateReleaseNotesQueryValidator : AbstractValidator<GenerateReleaseNotesQuery>
{
    public GenerateReleaseNotesQueryValidator()
    {
        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required.")
            .MaximumLength(50).WithMessage("Version must not exceed 50 characters.");

        RuleFor(x => x.CompletedTasks)
            .NotEmpty().WithMessage("At least one completed task is required to generate release notes.");
    }
}
