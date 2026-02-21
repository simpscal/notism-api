using Notism.Application.Order.Models;
using Notism.Shared.Models;

namespace Notism.Application.Order.AdminOrdersForKanban;

public record AdminOrdersForKanbanResponse : PagedResult<AdminOrderResponse>;