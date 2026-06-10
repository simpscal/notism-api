using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Food.Common;
using Notism.Domain.Common.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminDeleteCustomisationGroup;

public class AdminDeleteCustomisationGroupHandler : IRequestHandler<AdminDeleteCustomisationGroupRequest>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<AdminDeleteCustomisationGroupHandler> _logger;
    private readonly IMessages _messages;

    public AdminDeleteCustomisationGroupHandler(
        IRepository<Domain.Food.Food> foodRepository,
        ILogger<AdminDeleteCustomisationGroupHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(AdminDeleteCustomisationGroupRequest request, CancellationToken cancellationToken)
    {
        var spec = new FoodWithCustomisationsByIdSpecification(request.FoodId);
        var food = await _foodRepository.FindByExpressionAsync(spec)
            ?? throw new NotFoundException(_messages.FoodNotFound);

        var group = food.CustomisationGroups.FirstOrDefault(g => g.Id == request.GroupId)
            ?? throw new NotFoundException(_messages.CustomisationGroupNotFound);

        food.RemoveCustomisationGroup(group.Id);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted customisation group {GroupId} from food {FoodId}",
            request.GroupId,
            request.FoodId);
    }
}