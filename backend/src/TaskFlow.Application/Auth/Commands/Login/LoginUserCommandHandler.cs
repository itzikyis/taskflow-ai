using MediatR;
using TaskFlow.Application.Auth.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Auth.Commands.Login;

/// <summary>Handles user login; verifies password and issues a JWT.</summary>
public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService)
    : IRequestHandler<LoginUserCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Look up by email
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Result<AuthTokenDto>.Failure(AuthErrors.InvalidCredentials);

        // 2. Verify password (constant-time BCrypt compare)
        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthTokenDto>.Failure(AuthErrors.InvalidCredentials);

        // 3. Issue JWT
        var token = jwtService.GenerateToken(user);

        return Result<AuthTokenDto>.Success(
            new AuthTokenDto(token, user.Id, user.Email, user.DisplayName));
    }
}
