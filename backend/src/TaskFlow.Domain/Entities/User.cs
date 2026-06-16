using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Aggregate root representing an application user.
/// The password hash is stored — never the plaintext password.
/// </summary>
public sealed class User : AggregateRoot
{
    private User() { } // EF Core

    private User(Guid id, string email, string displayName, string passwordHash)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserRegisteredEvent(id, email));
    }

    /// <summary>Gets the user's email address (unique, used as login).</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Gets the user's display name.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Gets the BCrypt password hash. Never expose this to clients.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the account was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Creates a new <see cref="User"/>. The caller is responsible for passing
    /// an already-hashed password (never store raw passwords).
    /// </summary>
    public static Result<User> Create(string email, string displayName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.Failure(AuthErrors.EmailRequired);

        if (email.Length > 254)
            return Result<User>.Failure(AuthErrors.EmailTooLong);

        if (!email.Contains('@'))
            return Result<User>.Failure(AuthErrors.EmailInvalid);

        if (string.IsNullOrWhiteSpace(displayName))
            return Result<User>.Failure(AuthErrors.DisplayNameRequired);

        if (displayName.Length > 100)
            return Result<User>.Failure(AuthErrors.DisplayNameTooLong);

        var user = new User(Guid.NewGuid(), email.Trim().ToLowerInvariant(), displayName.Trim(), passwordHash);
        return Result<User>.Success(user);
    }
}
