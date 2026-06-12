using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminUpdateCustomisationOption;

public class AdminUpdateCustomisationOptionHandler : IRequestHandler<AdminUpdateCustomisationOptionRequest, AdminUpdateCustomisationOptionResponse>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminUpdateCustomisationOptionHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateCustomisationOptionHandler(
        IRepository<Domain.Food.Food> foodRepository,
        IReadDbContext readDbContext,
        ILogger<AdminUpdateCustomisationOptionHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUpdateCustomisationOptionResponse> Handle(
        AdminUpdateCustomisationOptionRequest request,
        CancellationToken cancellationToken)
    {
        var food = await _readDbContext.Set<Domain.Food.Food>(tracking: true)
            .Where(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include("CustomisationGroups.Options")
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.FoodNotFound);

        var group = food.CustomisationGroups.FirstOrDefault(g => g.Id == request.GroupId)
            ?? throw new NotFoundException(_messages.CustomisationGroupNotFound);

        var option = group.Options.FirstOrDefault(o => o.Id == request.OptionId)
            ?? throw new NotFoundException(_messages.CustomisationOptionNotFound);

        option.Update(request.Label, request.Surcharge, request.DisplayOrder);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated option {OptionId} in group {GroupId} on food {FoodId}",
            option.Id,
            group.Id,
            food.Id);

        return AdminUpdateCustomisationOptionResponse.FromDomain(option);
    }
}