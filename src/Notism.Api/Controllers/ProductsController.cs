using MediatR;

using Microsoft.AspNetCore.Mvc;

using Notism.Application.Products.Commands.CreateProduct;
using Notism.Application.Products.Queries.ListProducts;
using Notism.Shared.Interfaces;
using Notism.Shared.Models;

namespace Notism.Api.Controllers;

[Route("api/products")]
public class ProductsController(IMediator mediator, IFileStorageService fileStorageService) : ApiController
{
    [HttpPost]
    public Task<string> AddProduct(CreateProductCommand request)
    {
        return mediator.Send(request);
    }

    [HttpPost("filter")]
    public Task<PagedResult<ListProductsDto>> GetProducts([FromBody] ListProductsQuery request)
    {
        return mediator.Send(request);
    }

    [HttpPost("presigned-url")]
    public Task<string> GetPresignedUrl([FromBody] PresignedUrl request)
    {
        return fileStorageService.GetPresignedUrlAsync(
            request.FileName,
            "products",
            request.ContentType,
            TimeSpan.FromMinutes(15));
    }
}