using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Cash On Delivery gateway — immediately finalizes the order without external payment.
/// </summary>
public class CodGatewayService : IPaymentGateway
{
    private readonly IOrderFinalizationService _orderFinalizer;
    private readonly ISiteSettingsService _siteSettings;
    private readonly ILogger<CodGatewayService> _logger;

    public string GatewayName => "CashOnDelivery";

    public CodGatewayService(
        IOrderFinalizationService orderFinalizer,
        ISiteSettingsService siteSettings,
        ILogger<CodGatewayService> logger)
    {
        _orderFinalizer = orderFinalizer;
        _siteSettings = siteSettings;
        _logger = logger;
    }

    public async Task<(string? authorizationUrl, Dictionary<string, string>? postData)> CreatePaymentTransactionAsync(Order order)
    {
        // Immediately finalize the order (deduct stock, send emails, set status)
        await _orderFinalizer.FinalizePaymentAsync(order.PaymentReference, "COD-" + order.Id);

        _logger.LogInformation("COD order {OrderId} finalized immediately.", order.Id);

        var settings = await _siteSettings.GetSettingsAsync();
        var publicUrl = settings.GetValueOrDefault("PublicUrl") ?? "http://localhost:5106";

        // Redirect to confirmation page
        var confirmationUrl = $"{publicUrl}/Orders/Confirmation?reference={order.PaymentReference}";
        return (confirmationUrl, null);
    }

    public async Task<(Order? Order, string? AuthorizationUrl)> CreateRetryPaymentTransactionAsync(int orderId)
    {
        // COD doesn't need retries — orders are always finalized immediately
        await Task.CompletedTask;
        return (null, null);
    }

    public Task<Order?> HandleWebhookAsync(string jsonPayload, string signature)
    {
        // COD has no webhooks
        return Task.FromResult<Order?>(null);
    }

    public Task<string> RefundOrderAsync(Order order)
    {
        // COD refunds are handled manually
        return Task.FromResult("COD orders are refunded manually.");
    }
}
