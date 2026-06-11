using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.GetFoodById;

public class GetFoodByIdHandler : IRequestHandler<GetFoodByIdRequest, GetFoodByIdResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetFoodByIdHandler> _logger;
    private readonly IMessages _messages;

    public GetFoodByIdHandler(
        IReadDbContext readDbContext,
        IStorageService storageService,
        ILogger<GetFoodByIdHandler> logger,
        IMessages messages)
    {
        _readDbContext = readDbContext;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task<GetFoodByIdResponse> Handle(
        GetFoodByIdRequest request,
        CancellationToken cancellationToken)
    {
        var food = await new GetFoodByIdQuery(_readDbContext).ExecuteAsync(request.FoodId, cancellationToken);
        if (food == null)
        {
            throw new ResultFailureException(_messages.FoodNotFound);
        }

        _logger.LogInformation("Retrieved food {FoodId}", request.FoodId);

        return GetFoodByIdResponse.FromDomain(food, _storageService, request.IncludeEmptyGroups);
    }
}