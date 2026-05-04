using System;
using System.Text;

namespace Core.Entities.OrderAggregate;

public class ShippingAddress
{
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }

    public string? PhoneNumber { get; set; }
    public string? DeliveryNotes { get; set; }

    public string ToHtmlString()
    {
        var sb = new StringBuilder();
        sb.Append($"{Name} {LastName}<br/>");
        sb.Append($"{Line1}<br/>");
        if (!string.IsNullOrEmpty(Line2))
        {
            sb.Append($"{Line2}<br/>");
        }
        sb.Append($"{City}, {State} {PostalCode}<br/>");
        sb.Append(Country);
        return sb.ToString();
    }
}
