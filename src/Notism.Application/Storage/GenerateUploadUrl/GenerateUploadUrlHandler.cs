using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Shared.Extensions;

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
        var folderName = GetFolderName(request.Type);
        var (uploadUrl, fullKey) = await _storageService.GeneratePresignedUploadUrlAsync(
            folderName,
            request.FileName,
            request.ContentType,
            request.ExpirationMinutes);

        // Return file key without prefix for storage in database (client saves this)
        var prefixToStrip = folderName + "/";
        var fileKeyForDb = fullKey.StartsWith(prefixToStrip, StringComparison.Ordinal)
            ? fullKey.Substring(prefixToStrip.Length)
            : fullKey;

        _logger.LogInformation(
            "Generated presigned upload URL for file: {FileName}, ContentType: {ContentType}, Key: {FileKey}",
            request.FileName,
            request.ContentType,
            fileKeyForDb);

        return new GenerateUploadUrlResponse
        {
            UploadUrl = uploadUrl,
            FileKey = fileKeyForDb,
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpirationMinutes),
        };
    }

    private static string GetFolderName(string type)
    {
        return type.ToEnum<UploadType>().GetStringValue();
    }
}