using AutoMapper;

using Order.Application.Products.Queries.ListProducts;
using Order.Domain.Product;

namespace Order.Application.Products;

public class ProductsMapper : Profile
{
    public ProductsMapper()
    {
        CreateMap<Product, ListProductsDto>();
    }
}