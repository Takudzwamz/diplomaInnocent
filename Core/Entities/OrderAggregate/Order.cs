using System;
using Core.Interfaces;

namespace Core.Entities.OrderAggregate;

public class Order : BaseEntity, IDtoConvertible
{
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public required string BuyerEmail { get; set; }
    public ShippingAddress ShippingAddress { get; set; } = null!;
    public int DeliveryMethodId { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; } = null!;
    public PaymentSummary? PaymentSummary { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public string? CouponCode { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    // public required string PaymentIntentId { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.AwaitingProcessing;
    public required string PaymentReference { get; set; }

    public string? GatewayTransactionId { get; set; }

    public string? PaymentGatewayName { get; set; }

    public List<TrackingEvent> TrackingEvents { get; set; } = [];

    public decimal GetTotal()
    {
        return Subtotal - Discount + DeliveryMethod.Price;
    }
}
