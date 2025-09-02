using AutoMapper;

using Notism.Application.Products.Queries.ListProducts;
using Notism.Domain.Product;

namespace Notism.Application.Products;

public class ProductsMapper : Profile
{
    public ProductsMapper()
    {
        CreateMap<Product, ListProductsDto>();
    }
}