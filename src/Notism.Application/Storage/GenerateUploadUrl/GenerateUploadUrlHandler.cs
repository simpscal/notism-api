using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;

namespace Notism.Application.Storage.GenerateUploadUrl;

public class GenerateUploadUrlHandler : IRequestHandler<GenerateUploadUrlRequest, GenerateUploadUrlResponse>
{
    private readonly IStorageService _storageService;
    private readonly ILogger<GenerateUploadUrlHandler> _logger;

    public GenerateUploadUrlHandler(
        IStorageService storageService,
        ILogger<GenerateUploadUrlHandler> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GenerateUploadUrlResponse> Handle(
        GenerateUploadUrlRequest request,
        CancellationToken cancellationToken)
    {
        var (uploadUrl, fileKey) = await _storageService.GeneratePresignedUploadUrlAsync(
            request.FileName,
            request.ContentType,
            request.ExpirationMinutes);

        _logger.LogInformation(
            "Generated presigned upload URL for file: {FileName}, ContentType: {ContentType}, Key: {FileKey}",
            request.FileName,
            request.ContentType,
            fileKey);

        return new GenerateUploadUrlResponse
        {
            UploadUrl = uploadUrl,
            FileKey = fileKey,
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpirationMinutes),
        };
    }
}
