using System;

namespace Core.Entities.OrderAggregate;

public class OrderItem : BaseEntity
{
    public ProductItemOrdered ItemOrdered { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public Order Order { get; set; } = null!;
    public int OrderId { get; set; }
}
