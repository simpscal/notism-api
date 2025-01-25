using MediatR;

using Order.Domain.Category;
using Order.Domain.Common.Enums;
using Order.Domain.Product;
using Order.Domain.ProductColor;
using Order.Domain.ProductSize;
using Order.Domain.SubCategory;
using Order.Shared.Extensions;

namespace Order.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ISubCategoryRepository subCategoryRepository,
    IProductColorRepository productColorRepository,
    IProductSizeRepository productSizeRepository)
    : IRequestHandler<CreateProductCommand, string>
{
    public async Task<string> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var productColorsTask = productColorRepository.GetListAsync(
            request.Colors.Select(color => color.ToEnum<ColorType>()));
        var productSizesTask = productSizeRepository.GetListAsync(
            request.Sizes.Select(size => size.ToEnum<SizeType>()));

        var categoryIdTask = categoryRepository.GetIdAsync(
            request.Category.ToEnum<CategoryType>());
        var subCategoryIdTask = subCategoryRepository.GetIdAsync(
            request.SubCategory.ToEnum<SubCategoryType>());

        await Task.WhenAll(productColorsTask, productSizesTask, categoryIdTask, subCategoryIdTask);

        var productId = await productRepository.AddAsync(
            new Product
            {
                Name = request.Name,
                Price = request.Price,
                ProductColors = productColorsTask.Result.ToList(),
                ProductSizes = productSizesTask.Result.ToList(),
                CategoryId = categoryIdTask.Result,
                SubCategoryId = subCategoryIdTask.Result,
            });

        await productRepository.SaveChangesAsync();

        return productId;
    }
}

public record CreateProductCommand(
    string Name,
    decimal Price,
    string[] Colors,
    string[] Sizes,
    string SubCategory,
    string Category)
    : IRequest<string>;