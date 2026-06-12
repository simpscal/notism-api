using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Food.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminDeleteFood;

public class AdminDeleteFoodHandler : IRequestHandler<AdminDeleteFoodRequest>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminDeleteFoodHandler> _logger;
    private readonly IMessages _messages;

    public AdminDeleteFoodHandler(
        IFoodRepository foodRepository,
        IReadDbContext readDbContext,
        ILogger<AdminDeleteFoodHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(AdminDeleteFoodRequest request, CancellationToken cancellationToken)
    {
        var food = await _readDbContext.Set<Domain.Food.Food>(tracking: true)
                .Where(f => f.Id == request.FoodId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.FoodNotFound);

        food.MarkAsDeleted();
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation("Deleted food {FoodId}", request.FoodId);
    }
}