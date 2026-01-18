using FluentValidation;

namespace Notism.Application.Storage.DeleteFile;

public class DeleteFileValidator : AbstractValidator<DeleteFileRequest>
{
    public DeleteFileValidator()
    {
        RuleFor(x => x.FileKey)
            .NotEmpty().WithMessage("File key is required.")
            .MaximumLength(1024).WithMessage("File key must not exceed 1024 characters.");
    }
}