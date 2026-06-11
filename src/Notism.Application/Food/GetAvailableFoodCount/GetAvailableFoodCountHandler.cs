using MediatR;

using Notism.Application.Common.Persistence;

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
        var count = await new GetAvailableFoodCountQuery(_readDbContext)
            .ExecuteAsync(request.Category?.Trim(), cancellationToken);

        return new GetAvailableFoodCountResponse { Count = count };
    }
}
