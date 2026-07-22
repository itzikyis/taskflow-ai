using FluentValidation;

namespace TaskFlow.Application.AuditTrail.Queries.GetByActor;

/// <summary>Validates <see cref="GetAuditByActorQuery"/> pagination parameters.</summary>
public sealed class GetAuditByActorQueryValidator : AbstractValidator<GetAuditByActorQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetAuditByActorQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
