using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;

namespace Notism.Application.Cart.GetCartItems;

public class GetCartItemsHandler : IRequestHandler<GetCartItemsRequest, GetCartItemsResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetCartItemsHandler> _logger;

    public GetCartItemsHandler(
        IReadDbContext readDbContext,
        IStorageService storageService,
        ILogger<GetCartItemsHandler> logger)
    {
        _readDbContext = readDbContext;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetCartItemsResponse> Handle(
        GetCartItemsRequest request,
        CancellationToken cancellationToken)
    {
        var cartItems = await _readDbContext.Set<CartItem>()
            .Where(c => c.UserId == request.UserId)
            .Include(c => c.Food)
            .Include("Food.Category")
            .Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include("Food.CustomisationGroups.Options")
            .Include(c => c.Customisations)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} cart items for user {UserId}", cartItems.Count, request.UserId);

        return GetCartItemsResponse.FromDomain(cartItems, _storageService);
    }
}