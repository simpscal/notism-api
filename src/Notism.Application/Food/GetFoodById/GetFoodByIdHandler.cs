using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Repositories;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.GetFoodById;

public class GetFoodByIdHandler : IRequestHandler<GetFoodByIdRequest, GetFoodByIdResponse>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetFoodByIdHandler> _logger;
    private readonly IMessages _messages;

    public GetFoodByIdHandler(
        IRepository<Domain.Food.Food> foodRepository,
        IStorageService storageService,
        ILogger<GetFoodByIdHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task<GetFoodByIdResponse> Handle(
        GetFoodByIdRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include(f => f.Images)
            .Include(f => f.Category!)
            .Include("CustomisationGroups.Options");
        var food = await _foodRepository.FindByExpressionAsync(specification);
        if (food == null)
        {
            throw new ResultFailureException(_messages.FoodNotFound);
        }

        _logger.LogInformation("Retrieved food {FoodId}", request.FoodId);

        return GetFoodByIdResponse.FromDomain(food, _storageService, request.IncludeEmptyGroups);
    }
}