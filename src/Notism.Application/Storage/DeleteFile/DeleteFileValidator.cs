using FluentValidation;

using Notism.Application.Storage.GenerateUploadUrl;

using Notism.Shared.Extensions;

namespace Notism.Application.Storage.DeleteFile;

public class DeleteFileValidator : AbstractValidator<DeleteFileRequest>
{
    public DeleteFileValidator()
    {
        RuleFor(x => x.FileKey)
            .NotEmpty().WithMessage("File key is required.")
            .MaximumLength(1024).WithMessage("File key must not exceed 1024 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Storage type is required when deleting a file.")
            .Must(type => type.ExistInEnum<UploadType>())
            .WithMessage("Type must be one of: avatar, food.");
    }
}