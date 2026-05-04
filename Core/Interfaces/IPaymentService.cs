using Core.Entities.OrderAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Creates a payment transaction using the specified gateway.
    /// </summary>
    Task<(string? authorizationUrl, Dictionary<string, string>? postData)> CreatePaymentTransactionAsync(Order order);
    
    /// <summary>
    /// Routes an incoming webhook to the correct gateway for processing.
    /// </summary>
    Task<Order?> HandleWebhookAsync(string gatewayName, string jsonPayload, string signature);
    
    /// <summary>
    /// Retries a failed payment for an existing order.
    /// </summary>
    Task<(Order? Order, string? AuthorizationUrl)> CreateRetryPaymentTransactionAsync(string gatewayName, int orderId);

    /// <summary>
    /// Issues a refund via the specified gateway.
    /// </summary>
    // Task<string> RefundPaymentAsync(string gatewayName, string transactionReference);
    
    /// <summary>
    /// Issues a refund for the specified order via the specified gateway.
    /// </summary>  
    /// <returns>A string message indicating the result of the refund.</returns>
    Task<string> RefundOrderAsync(Order order);

    /// <summary>
    /// Gets a list of all payment gateways that are configured and enabled in the admin settings.
    /// </summary>
    IEnumerable<string> GetEnabledGateways();
    
    // --- THE OLD METHODS BELOW THIS LINE HAVE BEEN DELETED ---
}