using FluentValidation;

namespace TaskFlow.Application.AI.Queries.GetStatusDigest;

/// <summary>Validates <see cref="GetStatusDigestQuery"/>.</summary>
public sealed class GetStatusDigestQueryValidator : AbstractValidator<GetStatusDigestQuery>
{
    /// <summary>Initialises the validation rules.</summary>
    public GetStatusDigestQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.PeriodDays).InclusiveBetween(1, 90);
    }
}
