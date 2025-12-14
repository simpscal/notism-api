using MediatR;

namespace Notism.Application.Period.GetPeriods;

public class GetPeriodsRequest : IRequest<List<GetPeriodsResponse>>
{
}

