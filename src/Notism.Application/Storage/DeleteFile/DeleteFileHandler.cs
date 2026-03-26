using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Shared.Exceptions;

namespace Notism.Application.Storage.DeleteFile;

public class DeleteFileHandler : IRequestHandler<DeleteFileRequest>
{
    private readonly IStorageService _storageService;
    private readonly ILogger<DeleteFileHandler> _logger;
    private readonly IMessages _messages;

    public DeleteFileHandler(
        IStorageService storageService,
        ILogger<DeleteFileHandler> logger,
        IMessages messages)
    {
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(DeleteFileRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting file with key: {FileKey}", request.FileKey);

        var success = await _storageService.DeleteFileAsync(request.FileKey, request.Type);

        if (!success)
        {
            _logger.LogWarning("Failed to delete file: {FileKey}", request.FileKey);
            throw new ResultFailureException(_messages.FailedToDeleteFile);
        }

        _logger.LogInformation("Successfully deleted file: {FileKey}", request.FileKey);
    }
}