using FluentValidation;

namespace TaskFlow.Application.TimeTracking.Queries.GetTimeReport;

/// <summary>Validates <see cref="GetTimeReportQuery"/>.</summary>
public sealed class GetTimeReportQueryValidator : AbstractValidator<GetTimeReportQuery>
{
    public GetTimeReportQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
