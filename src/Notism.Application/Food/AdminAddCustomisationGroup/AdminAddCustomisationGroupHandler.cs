using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Repositories;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminAddCustomisationGroup;

public class AdminAddCustomisationGroupHandler : IRequestHandler<AdminAddCustomisationGroupRequest, AdminAddCustomisationGroupResponse>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<AdminAddCustomisationGroupHandler> _logger;
    private readonly IMessages _messages;

    public AdminAddCustomisationGroupHandler(
        IRepository<Domain.Food.Food> foodRepository,
        ILogger<AdminAddCustomisationGroupHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminAddCustomisationGroupResponse> Handle(
        AdminAddCustomisationGroupRequest request,
        CancellationToken cancellationToken)
    {
        var food = await _foodRepository.GetForUpdateAsync(
                f => f.Id == request.FoodId && !f.IsDeleted,
                includes => includes.Include("CustomisationGroups.Options"))
            ?? throw new NotFoundException(_messages.FoodNotFound);

        var group = FoodCustomisationGroup.Create(
            food.Id,
            request.Label,
            request.IsRequired,
            request.DisplayOrder);

        food.AddCustomisationGroup(group);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Added customisation group {GroupId} to food {FoodId}",
            group.Id,
            food.Id);

        return AdminAddCustomisationGroupResponse.FromDomain(group);
    }
}