using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Generates JWT access tokens for authenticated users.</summary>
public interface IJwtService
{
    /// <summary>Creates a signed JWT string for the given user.</summary>
    string GenerateToken(User user);
}
