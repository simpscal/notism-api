using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.DeleteFood;

public class DeleteFoodHandler : IRequestHandler<DeleteFoodRequest>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<DeleteFoodHandler> _logger;

    public DeleteFoodHandler(
        IFoodRepository foodRepository,
        ILogger<DeleteFoodHandler> logger)
    {
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task Handle(DeleteFoodRequest request, CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Notism.Domain.Food.Food>(f => f.Id == request.FoodId);
        var food = await _foodRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException("Food not found");

        food.MarkAsDeleted();
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation("Deleted food {FoodId}", request.FoodId);
    }
}
