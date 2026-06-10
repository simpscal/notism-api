using MediatR;

namespace Notism.Application.Order.AdminGetTodaySales;

public sealed record AdminGetTodaySalesRequest : IRequest<AdminGetTodaySalesResponse>;