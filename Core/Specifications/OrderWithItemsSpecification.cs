// Core/Specifications/OrderWithItemsSpecification.cs

using Core.Entities.OrderAggregate;

namespace Core.Specifications;

public class OrderWithItemsSpecification : BaseSpecification<Order>
{
    public OrderWithItemsSpecification(string paymentReference)
        : base(o => o.PaymentReference == paymentReference)
    {
        AddInclude(o => o.OrderItems);
        AddInclude(o => o.DeliveryMethod);
    }

    public OrderWithItemsSpecification(int orderId)
        : base(o => o.Id == orderId)
    {
        AddInclude(o => o.OrderItems);
        AddInclude(o => o.DeliveryMethod);
    }
}