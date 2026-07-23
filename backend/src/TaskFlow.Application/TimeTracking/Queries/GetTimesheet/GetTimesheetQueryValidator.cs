using FluentValidation;

namespace TaskFlow.Application.TimeTracking.Queries.GetTimesheet;

/// <summary>Validates <see cref="GetTimesheetQuery"/>.</summary>
public sealed class GetTimesheetQueryValidator : AbstractValidator<GetTimesheetQuery>
{
    public GetTimesheetQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
