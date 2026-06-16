namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for attachment management.</summary>
public static class AttachmentErrors
{
    /// <summary>Returned when the file name is empty or whitespace.</summary>
    public static readonly Error FileNameRequired = new("Attachment.FileNameRequired", "File name is required.");

    /// <summary>Returned when the file name exceeds 255 characters.</summary>
    public static readonly Error FileNameTooLong = new("Attachment.FileNameTooLong", "File name must not exceed 255 characters.");

    /// <summary>Returned when the content type is empty or whitespace.</summary>
    public static readonly Error ContentTypeRequired = new("Attachment.ContentTypeRequired", "Content type is required.");

    /// <summary>Returned when the storage URL is empty or whitespace.</summary>
    public static readonly Error StorageUrlRequired = new("Attachment.StorageUrlRequired", "Storage URL is required.");

    /// <summary>Returned when the file size exceeds 100 MB.</summary>
    public static readonly Error FileTooLarge = new("Attachment.FileTooLarge", "File size must not exceed 100 MB.");

    /// <summary>Returned when the requested attachment does not exist.</summary>
    public static readonly Error NotFound = new("Attachment.NotFound", "Attachment was not found.");

    /// <summary>Returned when the requester is not the uploader.</summary>
    public static readonly Error NotOwner = new("Attachment.NotOwner", "Only the uploader can delete this attachment.");
}
