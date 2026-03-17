using Notism.Application.Order.Models;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.Mappers;

public static class AdminOrderMapper
{
    public static AdminOrderResponse ToAdminOrderResponse(
        Domain.Order.Order order,
        Domain.User.User? user)
    {
        return new AdminOrderResponse
        {
            Id = order.Id,
            SlugId = order.SlugId,
            UserId = order.UserId,
            UserEmail = user?.Email.Value ?? string.Empty,
            UserName = GetUserName(user),
            TotalAmount = order.TotalAmount,
            DeliveryStatus = order.DeliveryStatus.GetStringValue(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            TotalItems = order.Items.Count,
        };
    }

    private static string GetUserName(Domain.User.User? user)
    {
        return user == null ||
               (string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName))
            ? string.Empty
            : $"{user.FirstName} {user.LastName}".Trim();
    }
}