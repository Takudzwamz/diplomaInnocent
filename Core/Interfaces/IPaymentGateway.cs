using Core.Entities;
using Core.Entities.OrderAggregate;
using System.Threading.Tasks;

namespace Core.Interfaces;

/// <summary>
/// Defines the contract for a payment gateway "strategy".
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// A unique name to identify this gateway, e.g., "Paystack" or "PayFast".
    /// </summary>
    string GatewayName { get; }

    /// <summary>
    /// Creates a payment transaction with the provider.
    /// </summary>
    /// <returns>A tuple containing success status, the URL to redirect the user to, and a payment reference.</returns>
    // Task<(bool success, string? authorizationUrl, string? paymentReference)> CreatePaymentTransactionAsync(ShoppingCart cart, string email);
    Task<(string? authorizationUrl, Dictionary<string, string>? postData)> CreatePaymentTransactionAsync(Order order);

    /// <summary>
    /// Retries a failed payment for an existing order.
    /// </summary>
    Task<(Order? Order, string? AuthorizationUrl)> CreateRetryPaymentTransactionAsync(int orderId);

    /// <summary>
    /// Handles an incoming webhook from the provider to confirm payment.
    /// </summary>
    /// <returns>The updated Order object if successful, or null if not.</returns>
    Task<Order?> HandleWebhookAsync(string jsonPayload, string signature);

    /// <summary>
    /// Issues a refund via the payment provider.
    /// </summary>
    /// <returns>A string message indicating the result of the refund.</returns>
    // Task<string> RefundPaymentAsync(string transactionReference);
    /// <summary>
    /// Issues a refund for the specified order via the payment provider.
    /// </summary>  
    /// <returns>A string message indicating the result of the refund.</returns>
    Task<string> RefundOrderAsync(Order order);
}