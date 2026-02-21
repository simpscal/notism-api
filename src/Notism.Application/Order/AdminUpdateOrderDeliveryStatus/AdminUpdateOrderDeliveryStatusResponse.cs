using Notism.Application.Order.Models;

namespace Notism.Application.Order.AdminUpdateOrderDeliveryStatus;

public class AdminUpdateOrderDeliveryStatusResponse : AdminOrderResponse
{
    public AdminUpdateOrderDeliveryStatusResponse()
    {
    }

    public AdminUpdateOrderDeliveryStatusResponse(AdminOrderResponse source)
    {
        Id = source.Id;
        SlugId = source.SlugId;
        UserId = source.UserId;
        UserEmail = source.UserEmail;
        UserName = source.UserName;
        TotalAmount = source.TotalAmount;
        DeliveryStatus = source.DeliveryStatus;
        CreatedAt = source.CreatedAt;
        UpdatedAt = source.UpdatedAt;
        TotalItems = source.TotalItems;
    }
}