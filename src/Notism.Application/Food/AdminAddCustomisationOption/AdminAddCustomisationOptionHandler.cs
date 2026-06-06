using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminAddCustomisationOption;

public class AdminAddCustomisationOptionHandler : IRequestHandler<AdminAddCustomisationOptionRequest, AdminAddCustomisationOptionResponse>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<AdminAddCustomisationOptionHandler> _logger;
    private readonly IMessages _messages;

    public AdminAddCustomisationOptionHandler(
        IRepository<Domain.Food.Food> foodRepository,
        ILogger<AdminAddCustomisationOptionHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminAddCustomisationOptionResponse> Handle(
        AdminAddCustomisationOptionRequest request,
        CancellationToken cancellationToken)
    {
        var spec = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include("CustomisationGroups.Options");
        var food = await _foodRepository.FindByExpressionAsync(spec)
            ?? throw new NotFoundException(_messages.FoodNotFound);

        var group = food.CustomisationGroups.FirstOrDefault(g => g.Id == request.GroupId)
            ?? throw new NotFoundException(_messages.CustomisationGroupNotFound);

        group.AddOption(request.Label, request.Surcharge, request.DisplayOrder);
        await _foodRepository.SaveChangesAsync();

        var option = group.Options.Last();

        _logger.LogInformation(
            "Added option {OptionId} to group {GroupId} on food {FoodId}",
            option.Id,
            group.Id,
            food.Id);

        return new AdminAddCustomisationOptionResponse
        {
            Id = option.Id,
            GroupId = option.GroupId,
            Label = option.Label,
            Surcharge = option.Surcharge,
            DisplayOrder = option.DisplayOrder,
        };
    }
}