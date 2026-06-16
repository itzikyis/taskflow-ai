using FluentValidation;

namespace TaskFlow.Application.Auth.Commands.Login;

/// <summary>Validates <see cref="LoginUserCommand"/> before the handler runs.</summary>
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}
