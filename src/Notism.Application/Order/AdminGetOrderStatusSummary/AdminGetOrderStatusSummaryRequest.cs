using MediatR;

namespace Notism.Application.Order.AdminGetOrderStatusSummary;

public sealed record AdminGetOrderStatusSummaryRequest : IRequest<AdminGetOrderStatusSummaryResponse>;