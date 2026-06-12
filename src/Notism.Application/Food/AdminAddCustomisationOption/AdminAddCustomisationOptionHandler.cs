using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminAddCustomisationOption;

public class AdminAddCustomisationOptionHandler : IRequestHandler<AdminAddCustomisationOptionRequest, AdminAddCustomisationOptionResponse>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminAddCustomisationOptionHandler> _logger;
    private readonly IMessages _messages;

    public AdminAddCustomisationOptionHandler(
        IRepository<Domain.Food.Food> foodRepository,
        IReadDbContext readDbContext,
        ILogger<AdminAddCustomisationOptionHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminAddCustomisationOptionResponse> Handle(
        AdminAddCustomisationOptionRequest request,
        CancellationToken cancellationToken)
    {
        var food = await _readDbContext.BuildGraphQuery<Domain.Food.Food>(
                f => f.Id == request.FoodId && !f.IsDeleted,
                includes => includes.Include("CustomisationGroups.Options"),
                tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
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

        return AdminAddCustomisationOptionResponse.FromDomain(option);
    }
}