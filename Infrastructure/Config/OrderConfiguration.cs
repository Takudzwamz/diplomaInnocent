using System;
using Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.OwnsOne(x => x.ShippingAddress, o => o.WithOwner());
        builder.OwnsOne(x => x.PaymentSummary, o => o.WithOwner());
        builder.Property(x => x.Status).HasConversion(
            o => o.ToString(),
            o => (OrderStatus)Enum.Parse(typeof(OrderStatus), o)
        );
        builder.Property(x => x.DeliveryStatus)
            .HasConversion(
                o => o.ToString(),
                o => (DeliveryStatus)Enum.Parse(typeof(DeliveryStatus), o)
            );
        builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Discount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.OrderDate).HasConversion(
            x => x.ToUniversalTime(),
            x => DateTime.SpecifyKind(x, DateTimeKind.Utc)
        );

        builder.HasMany(o => o.OrderItems)        // Order has many OrderItems
               .WithOne(oi => oi.Order)           // Each OrderItem has one Order
               .HasForeignKey(oi => oi.OrderId)   // The foreign key is OrderId
               .OnDelete(DeleteBehavior.Cascade);
    }
}
