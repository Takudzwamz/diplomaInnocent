using System;
using Core.Entities.OrderAggregate;

namespace Core.Specifications;

public class OrderSpecification : BaseSpecification<Order>
{
    public OrderSpecification(string email, OrderSpecParams orderParams)
        : base(x => x.BuyerEmail == email)
    {
        AddInclude(x => x.DeliveryMethod);
        AddInclude(x => x.OrderItems);
        AddOrderByDescending(x => x.OrderDate); // Default sort

        ApplyPaging(orderParams.PageSize * (orderParams.PageIndex - 1), orderParams.PageSize);

        if (!string.IsNullOrEmpty(orderParams.Sort))
        {
            switch (orderParams.Sort)
            {
                case "totalAsc":
                    AddOrderBy(o => o.Subtotal - o.Discount + o.DeliveryMethod.Price);
                    break;
                case "totalDesc":
                    AddOrderByDescending(o => o.Subtotal - o.Discount + o.DeliveryMethod.Price);
                    break;
                default:
                    AddOrderByDescending(x => x.OrderDate);
                    break;
            }
        }
    }


    public OrderSpecification(string email, int id) : base(x => x.BuyerEmail == email && x.Id == id)
    {
        AddInclude(o => o.OrderItems);
        AddInclude(o => o.DeliveryMethod);
        AddInclude(o => o.TrackingEvents);

    }

    public OrderSpecification(string paymentReference, bool isPaymentReference)
        : base(x => x.PaymentReference == paymentReference)
    {
        AddInclude(x => x.DeliveryMethod);
        AddInclude(x => x.OrderItems);
        AddInclude(x => x.TrackingEvents);
    }



    public OrderSpecification(OrderSpecParams specParams) : base(x =>
    (string.IsNullOrEmpty(specParams.Status) || x.Status == ParseStatus(specParams.Status)) &&
    (string.IsNullOrEmpty(specParams.CustomerEmail) || x.BuyerEmail == specParams.CustomerEmail) // <-- ADD THIS LINE
)
    {
        AddInclude(x => x.OrderItems);
        AddInclude(x => x.DeliveryMethod);
        ApplyPaging(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);

        // THIS IS THE FIX: All sorting logic is now handled in one place,
        // ensuring only a single OrderBy is applied.
        if (!string.IsNullOrEmpty(specParams.Sort))
        {
            switch (specParams.Sort)
            {
                case "totalAsc":
                    AddOrderBy(o => o.Subtotal - o.Discount + o.DeliveryMethod.Price);
                    break;
                case "totalDesc":
                    AddOrderByDescending(o => o.Subtotal - o.Discount + o.DeliveryMethod.Price);
                    break;
                default: // This handles "dateDesc" or any other value
                    AddOrderByDescending(x => x.OrderDate);
                    break;
            }
        }
        else // If no sort is specified at all, default to sorting by date
        {
            AddOrderByDescending(x => x.OrderDate);
        }
    }


    public OrderSpecification(int id, bool includeTracking) : base(o => o.Id == id)
    {
        if (includeTracking)
        {
            AddInclude(o => o.TrackingEvents);
        }

        AddInclude(o => o.DeliveryMethod);
    }


    public OrderSpecification(int id) : base(x => x.Id == id)
    {
        AddInclude("OrderItems");
        AddInclude("DeliveryMethod");
        AddInclude("TrackingEvents"); // This correctly includes the tracking data
                                      // The invalid sort line has been removed.
    }

    private static OrderStatus? ParseStatus(string status)
    {
        if (Enum.TryParse<OrderStatus>(status, true, out var result)) return result;
        return null;
    }

}
