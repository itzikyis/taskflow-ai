namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for the custom fields feature.</summary>
public static class CustomFieldErrors
{
    /// <summary>Raised when the project ID is empty.</summary>
    public static readonly Error ProjectIdRequired =
        new("CustomField.ProjectIdRequired", "A valid project ID is required.");

    /// <summary>Raised when the field name is missing.</summary>
    public static readonly Error NameRequired =
        new("CustomField.NameRequired", "Custom field name cannot be empty.");

    /// <summary>Raised when the field name exceeds the maximum allowed length.</summary>
    public static readonly Error NameTooLong =
        new("CustomField.NameTooLong", "Custom field name must not exceed 100 characters.");

    /// <summary>Raised when an unsupported field type is supplied.</summary>
    public static readonly Error InvalidFieldType =
        new("CustomField.InvalidFieldType", "Field type must be one of: Text, Number, Select, Date.");

    /// <summary>Raised when a custom field cannot be found.</summary>
    public static readonly Error NotFound =
        new("CustomField.NotFound", "The specified custom field was not found.");
}
