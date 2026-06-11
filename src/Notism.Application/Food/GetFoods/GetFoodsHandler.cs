using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;

namespace Notism.Application.Food.GetFoods;

public class GetFoodsHandler : IRequestHandler<GetFoodsRequest, GetFoodsResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetFoodsHandler> _logger;

    public GetFoodsHandler(
        IReadDbContext readDbContext,
        IStorageService storageService,
        ILogger<GetFoodsHandler> logger)
    {
        _readDbContext = readDbContext;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetFoodsResponse> Handle(
        GetFoodsRequest request,
        CancellationToken cancellationToken)
    {
        var keywordLower = request.Keyword?.ToLower();
        var categoryFilter = request.Category?.Trim();

        var (totalCount, projections) = await new GetFoodsQuery(_readDbContext).ExecuteAsync(
            categoryFilter,
            keywordLower,
            request.IsAvailable,
            request.SortBy,
            request.SortOrder,
            request.Skip,
            request.Take,
            cancellationToken);

        var items = projections
            .Select(proj => FoodItemResponse.FromProjection(proj, _storageService));

        _logger.LogInformation("Retrieved {Count} foods", projections.Count);

        return new GetFoodsResponse
        {
            TotalCount = totalCount,
            Items = items,
        };
    }
}