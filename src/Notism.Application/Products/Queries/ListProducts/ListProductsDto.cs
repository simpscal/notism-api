namespace Notism.Application.Products.Queries.ListProducts;

public class ListProductsDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
}