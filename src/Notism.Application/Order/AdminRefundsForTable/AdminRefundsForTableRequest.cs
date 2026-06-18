using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.Order.AdminRefundsForTable;

public record AdminRefundsForTableRequest : FilterParams, IRequest<AdminRefundsForTableResponse>
{
    public string? Status { get; set; }
}