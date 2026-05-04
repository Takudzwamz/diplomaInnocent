using Core.Entities.OrderAggregate;

namespace Core.Specifications;

/// <summary>
/// Specification to get orders with their DeliveryMethod included
/// Used for analytics and reporting where GetTotal() needs DeliveryMethod.Price
/// </summary>
public class OrdersWithDeliveryMethodSpecification : BaseSpecification<Order>
{
    public OrdersWithDeliveryMethodSpecification() : base()
    {
        AddInclude(o => o.DeliveryMethod);
    }

    public OrdersWithDeliveryMethodSpecification(OrderStatus status) 
        : base(o => o.Status == status)
    {
        AddInclude(o => o.DeliveryMethod);
    }
}

/// <summary>
/// Specification to get orders with their OrderItems and DeliveryMethod included
/// Used for detailed analytics that need item-level data
/// </summary>
public class OrdersWithItemsAndDeliveryMethodSpecification : BaseSpecification<Order>
{
    public OrdersWithItemsAndDeliveryMethodSpecification() : base()
    {
        AddInclude(o => o.DeliveryMethod);
        AddInclude(o => o.OrderItems);
    }

    public OrdersWithItemsAndDeliveryMethodSpecification(OrderStatus status) 
        : base(o => o.Status == status)
    {
        AddInclude(o => o.DeliveryMethod);
        AddInclude(o => o.OrderItems);
    }
}
