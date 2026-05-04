using Core.Entities.OrderAggregate;
using System.Threading.Tasks;

namespace Core.Interfaces;

public interface IOrderFinalizationService
{
    /// <summary>
    /// Finds an order by its payment reference, validates it,
    /// deducts stock, updates coupons, sends a confirmation email,
    /// and marks the order as 'PaymentReceived'.
    /// </summary>
    /// <param name="paymentReference">The unique payment reference from the gateway.</param>
    /// <returns>The updated Order, or null if not found or already processed.</returns>
    Task<Order?> FinalizePaymentAsync(string paymentReference, string? gatewayTransactionId);
}