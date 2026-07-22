using FluentValidation;

namespace TaskFlow.Application.Teams.Queries.GetTeamWorkload;

/// <summary>Validates <see cref="GetTeamWorkloadQuery"/>.</summary>
public sealed class GetTeamWorkloadQueryValidator : AbstractValidator<GetTeamWorkloadQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetTeamWorkloadQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
