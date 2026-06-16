using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Auth.Commands.Login;
using TaskFlow.Application.Auth.Commands.Register;
using TaskFlow.Application.Auth.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.API.Controllers;

/// <summary>Handles user registration and login.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>Registers a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterUserCommand(request.Email, request.DisplayName, request.Password),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == AuthErrors.EmailAlreadyTaken.Code
                ? Conflict(result.Error)
                : BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(Register), new { id = result.Value }, result.Value);
    }

    /// <summary>Logs in with email and password; returns a JWT access token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginUserCommand(request.Email, request.Password),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == AuthErrors.InvalidCredentials.Code
                ? Unauthorized(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (API layer only)
// ---------------------------------------------------------------------------

/// <summary>Payload for registering a new user.</summary>
public sealed record RegisterRequest(string Email, string DisplayName, string Password);

/// <summary>Payload for logging in.</summary>
public sealed record LoginRequest(string Email, string Password);
