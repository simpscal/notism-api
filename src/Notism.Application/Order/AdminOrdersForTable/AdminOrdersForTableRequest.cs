using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.Order.AdminOrdersForTable;

public record AdminOrdersForTableRequest : FilterParams, IRequest<AdminOrdersForTableResponse>
{
    public string? PaymentStatus { get; set; }
}