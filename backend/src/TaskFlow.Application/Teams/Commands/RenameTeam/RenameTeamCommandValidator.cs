using FluentValidation;

namespace TaskFlow.Application.Teams.Commands.RenameTeam;

/// <summary>Validates <see cref="RenameTeamCommand"/>.</summary>
public sealed class RenameTeamCommandValidator : AbstractValidator<RenameTeamCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public RenameTeamCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
