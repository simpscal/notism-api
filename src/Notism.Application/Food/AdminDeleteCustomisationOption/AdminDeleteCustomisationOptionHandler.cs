using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminDeleteCustomisationOption;

public class AdminDeleteCustomisationOptionHandler : IRequestHandler<AdminDeleteCustomisationOptionRequest>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<AdminDeleteCustomisationOptionHandler> _logger;
    private readonly IMessages _messages;

    public AdminDeleteCustomisationOptionHandler(
        IRepository<Domain.Food.Food> foodRepository,
        ILogger<AdminDeleteCustomisationOptionHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(AdminDeleteCustomisationOptionRequest request, CancellationToken cancellationToken)
    {
        var spec = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include("CustomisationGroups.Options");
        var food = await _foodRepository.FindByExpressionAsync(spec)
            ?? throw new NotFoundException(_messages.FoodNotFound);

        var group = food.CustomisationGroups.FirstOrDefault(g => g.Id == request.GroupId)
            ?? throw new NotFoundException(_messages.CustomisationGroupNotFound);

        var option = group.Options.FirstOrDefault(o => o.Id == request.OptionId)
            ?? throw new NotFoundException(_messages.CustomisationOptionNotFound);

        group.RemoveOption(option.Id);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted option {OptionId} from group {GroupId} on food {FoodId}",
            request.OptionId,
            request.GroupId,
            request.FoodId);
    }
}
