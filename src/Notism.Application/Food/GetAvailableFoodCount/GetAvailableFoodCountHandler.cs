using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;

using DomainFood = Notism.Domain.Food.Food;

namespace Notism.Application.Food.GetAvailableFoodCount;

public class GetAvailableFoodCountHandler : IRequestHandler<GetAvailableFoodCountRequest, GetAvailableFoodCountResponse>
{
    private readonly IReadDbContext _readDbContext;

    public GetAvailableFoodCountHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetAvailableFoodCountResponse> Handle(
        GetAvailableFoodCountRequest request,
        CancellationToken cancellationToken)
    {
        var category = request.Category?.Trim();

        var count = await _readDbContext.Set<DomainFood>()
            .Where(f => !f.IsDeleted
                && f.IsAvailable
                && (string.IsNullOrWhiteSpace(category) || (f.Category != null && f.Category.Name == category)))
            .CountAsync(cancellationToken);

        return new GetAvailableFoodCountResponse { Count = count };
    }
}
