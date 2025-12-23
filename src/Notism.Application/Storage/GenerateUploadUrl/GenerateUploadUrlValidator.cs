using FluentValidation;

using Notism.Shared.Extensions;

namespace Notism.Application.Storage.GenerateUploadUrl;

public class GenerateUploadUrlValidator : AbstractValidator<GenerateUploadUrlRequest>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    ];

    public GenerateUploadUrlValidator()
    {
        var validUploadTypes = Enum.GetNames<UploadType>();

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required")
            .Must(type => type.ExistInEnum<UploadType>())
            .WithMessage($"Type must be one of: {string.Join(", ", validUploadTypes)}");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.ExpirationMinutes)
            .InclusiveBetween(1, 1440)
            .WithMessage("Expiration must be between 1 and 1440 minutes (24 hours)");
    }
}