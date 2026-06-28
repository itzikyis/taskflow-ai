using FluentValidation;

namespace TaskFlow.Application.Teams.Commands.AddMember;

/// <summary>Validates <see cref="AddMemberCommand"/>.</summary>
public sealed class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public AddMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
