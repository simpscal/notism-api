using MediatR;

using Microsoft.AspNetCore.Mvc;

using Notism.Api.Models;
using Notism.Application.Storage.DeleteFile;
using Notism.Application.Storage.GenerateUploadUrl;

namespace Notism.Api.Endpoints;

public static class StorageEndpoints
{
    public static void MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/storage")
            .WithTags("Storage")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/presigned-url/upload", GenerateUploadUrlAsync)
            .WithName("GenerateUploadUrl")
            .WithSummary("Generate presigned URL for file upload")
            .WithDescription("Generates a presigned URL that allows secure file upload to S3")
            .Produces<GenerateUploadUrlResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapDelete("/file", DeleteFileAsync)
            .WithName("DeleteFile")
            .WithSummary("Delete a file from storage")
            .WithDescription("Deletes a file from S3 storage using the file key")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GenerateUploadUrlAsync(
        GenerateUploadUrlRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteFileAsync(
        [FromQuery] string fileKey,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = new DeleteFileRequest { FileKey = fileKey };
        await mediator.Send(request, cancellationToken);
        return Results.Ok();
    }
}
