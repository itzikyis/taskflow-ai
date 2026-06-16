using MediatR;
using TaskFlow.Application.Auth.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Auth.Commands.Login;

/// <summary>Authenticates a user and returns a JWT + profile.</summary>
public sealed record LoginUserCommand(
    string Email,
    string Password) : IRequest<Result<AuthTokenDto>>;
