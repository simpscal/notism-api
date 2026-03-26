using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminDeleteFood;

public class AdminDeleteFoodHandler : IRequestHandler<AdminDeleteFoodRequest>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<AdminDeleteFoodHandler> _logger;
    private readonly IMessages _messages;

    public AdminDeleteFoodHandler(
        IFoodRepository foodRepository,
        ILogger<AdminDeleteFoodHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(AdminDeleteFoodRequest request, CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Notism.Domain.Food.Food>(f => f.Id == request.FoodId);
        var food = await _foodRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException(_messages.FoodNotFound);

        food.MarkAsDeleted();
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation("Deleted food {FoodId}", request.FoodId);
    }
}