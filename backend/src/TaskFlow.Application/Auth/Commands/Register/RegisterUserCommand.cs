using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Auth.Commands.Register;

/// <summary>Registers a new user account and returns their id.</summary>
public sealed record RegisterUserCommand(
    string Email,
    string DisplayName,
    string Password) : IRequest<Result<Guid>>;
