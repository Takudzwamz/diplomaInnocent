namespace Core.Entities.OrderAggregate;

public enum DeliveryStatus
{
    AwaitingProcessing, // The default state before payment
    Processing,
    Shipped,
    OutForDelivery,
    Delivered
}