using System;
using Core.DTOs;
using Core.Entities.OrderAggregate;

namespace Core.Extensions;

public static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            BuyerEmail = order.BuyerEmail,
            OrderDate = order.OrderDate,
            ShippingAddress = order.ShippingAddress,
            PaymentSummary = order.PaymentSummary,
            DeliveryMethod = order.DeliveryMethod.Description,
            ShippingPrice = order.DeliveryMethod.Price,
            OrderItems = order.OrderItems.Select(x => x.ToDto()).ToList(),
            Subtotal = order.Subtotal,
            Discount = order.Discount,
            Status = order.Status.ToString(),
            PaymentReference = order.PaymentReference,
            DeliveryStatus = order.DeliveryStatus.ToString(),
            // It checks if there are tracking events and translates them
            TrackingHistory = order.TrackingEvents?.Select(te => new TrackingEventDto
            {
                EventDate = te.EventDate,
                Status = te.Status,
                Notes = te.Notes
            }).OrderByDescending(te => te.EventDate).ToList() ?? [],
            Total = order.GetTotal(),
            
        };
    }

    public static OrderItemDto ToDto(this OrderItem orderItem)
    {
        return new OrderItemDto
        {
            ProductId = orderItem.ItemOrdered.ProductId,
            ProductName = orderItem.ItemOrdered.ProductName,
            PictureUrl = orderItem.ItemOrdered.PictureUrl,
            Price = orderItem.Price,
            Quantity = orderItem.Quantity,
            SelectedOptions = orderItem.ItemOrdered.SelectedOptions

        };
    }
}