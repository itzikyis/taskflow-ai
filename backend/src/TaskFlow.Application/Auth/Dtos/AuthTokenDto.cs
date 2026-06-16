namespace TaskFlow.Application.Auth.Dtos;

/// <summary>Returned after a successful login.</summary>
public sealed record AuthTokenDto(
    string AccessToken,
    Guid UserId,
    string Email,
    string DisplayName);
