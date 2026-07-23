using FluentValidation;

namespace TaskFlow.Application.Reporting.Queries.GetProjectAnalytics;

/// <summary>Validates <see cref="GetProjectAnalyticsQuery"/>.</summary>
public sealed class GetProjectAnalyticsQueryValidator : AbstractValidator<GetProjectAnalyticsQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetProjectAnalyticsQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId must not be empty.");
    }
}
