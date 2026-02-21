using Notism.Application.Order.Models;
using Notism.Shared.Models;

namespace Notism.Application.Order.AdminOrdersForTable;

public record AdminOrdersForTableResponse : PagedResult<AdminOrderResponse>;