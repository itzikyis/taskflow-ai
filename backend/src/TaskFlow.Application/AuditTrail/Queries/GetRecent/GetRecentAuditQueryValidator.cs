using FluentValidation;

namespace TaskFlow.Application.AuditTrail.Queries.GetRecent;

/// <summary>Validates <see cref="GetRecentAuditQuery"/> pagination parameters.</summary>
public sealed class GetRecentAuditQueryValidator : AbstractValidator<GetRecentAuditQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetRecentAuditQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
