using FluentValidation;

namespace TaskFlow.Application.Attachments.Commands.AddAttachment;

/// <summary>Validates <see cref="AddAttachmentCommand"/>.</summary>
public sealed class AddAttachmentCommandValidator : AbstractValidator<AddAttachmentCommand>
{
    public AddAttachmentCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.StorageUrl).NotEmpty();
        RuleFor(x => x.FileSizeBytes).GreaterThan(0);
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.UploadedBy).NotEmpty();
    }
}
