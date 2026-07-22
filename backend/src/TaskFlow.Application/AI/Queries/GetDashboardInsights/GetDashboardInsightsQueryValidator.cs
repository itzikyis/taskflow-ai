using FluentValidation;

namespace TaskFlow.Application.AI.Queries.GetDashboardInsights;

/// <summary>Validates <see cref="GetDashboardInsightsQuery"/>.</summary>
public sealed class GetDashboardInsightsQueryValidator : AbstractValidator<GetDashboardInsightsQuery>
{
    /// <summary>Initialises the validation rules.</summary>
    public GetDashboardInsightsQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
