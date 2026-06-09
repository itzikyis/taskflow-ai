namespace TaskFlow.Domain.Common;

/// <summary>Represents a domain error with a code and human-readable description.</summary>
public sealed record Error(string Code, string Description)
{
    /// <summary>Sentinel value for a successful result (no error).</summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>General not-found error.</summary>
    public static readonly Error NotFound = new("General.NotFound", "The requested resource was not found.");

    /// <summary>General validation error.</summary>
    public static readonly Error Validation = new("General.Validation", "A validation error occurred.");
}
