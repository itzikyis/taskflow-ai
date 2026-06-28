using FluentValidation;

namespace TaskFlow.Application.Teams.Commands.CreateTeam;

/// <summary>Validates <see cref="CreateTeamCommand"/>.</summary>
public sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
