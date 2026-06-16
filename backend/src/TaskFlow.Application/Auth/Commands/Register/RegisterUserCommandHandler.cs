using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Auth.Commands.Register;

/// <summary>Handles user registration.</summary>
public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Check email uniqueness
        if (await userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            return Result<Guid>.Failure(AuthErrors.EmailAlreadyTaken);

        // 2. Hash the password
        var hash = passwordHasher.Hash(request.Password);

        // 3. Create domain entity (validates email/name rules)
        var userResult = User.Create(request.Email, request.DisplayName, hash);
        if (userResult.IsFailure)
            return Result<Guid>.Failure(userResult.Error);

        var user = userResult.Value!;

        // 4. Persist
        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
