using MediatR;

namespace Notism.Application.Storage.DeleteFile;

public class DeleteFileRequest : IRequest
{
    public required string FileKey { get; set; }
}