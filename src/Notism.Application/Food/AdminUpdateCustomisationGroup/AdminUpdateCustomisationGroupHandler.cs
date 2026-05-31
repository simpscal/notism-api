using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminUpdateCustomisationGroup;

public class AdminUpdateCustomisationGroupHandler : IRequestHandler<AdminUpdateCustomisationGroupRequest, AdminUpdateCustomisationGroupResponse>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<AdminUpdateCustomisationGroupHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateCustomisationGroupHandler(
        IRepository<Domain.Food.Food> foodRepository,
        ILogger<AdminUpdateCustomisationGroupHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUpdateCustomisationGroupResponse> Handle(
        AdminUpdateCustomisationGroupRequest request,
        CancellationToken cancellationToken)
    {
        var spec = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include("CustomisationGroups.Options");
        var food = await _foodRepository.FindByExpressionAsync(spec)
            ?? throw new NotFoundException(_messages.FoodNotFound);

        var group = food.CustomisationGroups.FirstOrDefault(g => g.Id == request.GroupId)
            ?? throw new NotFoundException(_messages.CustomisationGroupNotFound);

        group.Update(request.Label, request.IsRequired, request.DisplayOrder);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated customisation group {GroupId} on food {FoodId}",
            group.Id,
            food.Id);

        return new AdminUpdateCustomisationGroupResponse
        {
            Id = group.Id,
            FoodId = group.FoodId,
            Label = group.Label,
            IsRequired = group.IsRequired,
            DisplayOrder = group.DisplayOrder,
        };
    }
}
