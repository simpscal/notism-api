using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.Order.AdminOrdersForKanban;

public record AdminOrdersForKanbanRequest : FilterParams, IRequest<AdminOrdersForKanbanResponse>
{
    public string Status { get; set; } = string.Empty;
    public string? PaymentStatus { get; set; }
}