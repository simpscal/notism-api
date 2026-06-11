using MediatR;

using Notism.Application.Common.Persistence;
using Notism.Application.Food.Common;

namespace Notism.Application.Food.GetCategories;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesRequest, GetCategoriesResponse>
{
    private readonly IReadDbContext _readDbContext;

    public GetCategoriesHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetCategoriesResponse> Handle(
        GetCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var categories = await new GetCategoriesQuery(_readDbContext).ExecuteAsync(cancellationToken);
        var items = categories
            .Select(CategoryResponse.FromDomain)
            .ToList();

        return new GetCategoriesResponse
        {
            Items = items,
            TotalCount = items.Count,
        };
    }
}