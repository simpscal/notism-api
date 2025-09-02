using AutoMapper;

using MediatR;

using Notism.Domain.Product;
using Notism.Domain.Product.Models;
using Notism.Domain.Product.Specifications;
using Notism.Shared.Models;

namespace Notism.Application.Products.Queries.ListProducts;

public class ListProductsQueryHandler(IMapper mapper, IProductRepository productRepository)
    : IRequestHandler<ListProductsQuery, PagedResult<ListProductsDto>>
{
    public async Task<PagedResult<ListProductsDto>> Handle(
        ListProductsQuery request,
        CancellationToken cancellationToken)
    {
        var filterProductsSpecification = new FilterProductsSpecification(request);
        var pagedResult = await productRepository.FilterPagedByExpressionAsync(filterProductsSpecification, request);

        return new PagedResult<ListProductsDto>()
        {
            TotalCount = pagedResult.TotalCount,
            Items = pagedResult.Items.Select(product => mapper.Map<ListProductsDto>(product)),
        };
    }
}

public record ListProductsQuery : ProductFilterParams, IRequest<PagedResult<ListProductsDto>>;