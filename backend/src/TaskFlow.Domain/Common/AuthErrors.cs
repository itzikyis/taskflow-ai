namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for authentication and user management.</summary>
public static class AuthErrors
{
    public static readonly Error EmailRequired =
        new("Auth.EmailRequired", "Email address is required.");

    public static readonly Error EmailInvalid =
        new("Auth.EmailInvalid", "Email address is not valid.");

    public static readonly Error EmailTooLong =
        new("Auth.EmailTooLong", "Email address must not exceed 254 characters.");

    public static readonly Error PasswordTooShort =
        new("Auth.PasswordTooShort", "Password must be at least 8 characters.");

    public static readonly Error PasswordTooLong =
        new("Auth.PasswordTooLong", "Password must not exceed 100 characters.");

    public static readonly Error DisplayNameRequired =
        new("Auth.DisplayNameRequired", "Display name is required.");

    public static readonly Error DisplayNameTooLong =
        new("Auth.DisplayNameTooLong", "Display name must not exceed 100 characters.");

    public static readonly Error EmailAlreadyTaken =
        new("Auth.EmailAlreadyTaken", "An account with this email already exists.");

    public static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Email or password is incorrect.");

    public static readonly Error UserNotFound =
        new("Auth.UserNotFound", "User was not found.");
}
