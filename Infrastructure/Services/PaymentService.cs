using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Core.Specifications;
using Core.Entities;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unit;
    private readonly Dictionary<string, IPaymentGateway> _gateways;
    private readonly UserManager<AppUser> _userManager;
    private readonly ISiteSettingsService _siteSettings;

    public PaymentService(IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unit,
        IEnumerable<IPaymentGateway> gateways,
        UserManager<AppUser> userManager,
        ISiteSettingsService siteSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _unit = unit;
        _gateways = gateways.ToDictionary(g => g.GatewayName, g => g, StringComparer.OrdinalIgnoreCase);
        _userManager = userManager;
        _siteSettings = siteSettings;
    }

    // --- THIS IS THE FIX: ADDED THIS METHOD BACK ---
    /// <summary>
    /// Gets a specific gateway by its name.
    /// </summary>
    private IPaymentGateway GetGateway(string gatewayName)
    {
        if (string.IsNullOrEmpty(gatewayName) || !_gateways.ContainsKey(gatewayName))
        {
            throw new Exception($"Payment gateway '{gatewayName}' is not registered or not found.");
        }
        return _gateways[gatewayName];
    }

    /// <summary>
    /// Gets the single gateway marked as "Active" in the admin settings.
    /// </summary>
    private IPaymentGateway GetActiveGateway()
    {
        var settings = _siteSettings.GetSettingsAsync().Result;
        var activeGatewayName = settings.GetValueOrDefault("Payment_ActiveGateway", "Paystack");

        return GetGateway(activeGatewayName);
    }
    // --- END OF FIX ---

    public IEnumerable<string> GetEnabledGateways()
    {
        return new[] { GetActiveGateway().GatewayName };
    }

    public async Task<(string? authorizationUrl, Dictionary<string, string>? postData)> CreatePaymentTransactionAsync(Order order)
    {
        var gateway = GetActiveGateway();
        order.PaymentGatewayName = gateway.GatewayName;
        return await gateway.CreatePaymentTransactionAsync(order);
    }

    public async Task<Order?> HandleWebhookAsync(string gatewayName, string jsonPayload, string signature)
    {
        var gateway = GetGateway(gatewayName); // This now works
        return await gateway.HandleWebhookAsync(jsonPayload, signature);
    }

    public async Task<string> RefundOrderAsync(Order order)
    {
        if (string.IsNullOrEmpty(order.PaymentGatewayName))
        {
            return "Refund failed: Payment gateway name not found on order.";
        }
        var gateway = GetGateway(order.PaymentGatewayName);
        return await gateway.RefundOrderAsync(order);
    }

    public async Task<(Order? Order, string? AuthorizationUrl)> CreateRetryPaymentTransactionAsync(string gatewayName, int orderId)
    {
        var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) throw new Exception("User not found.");

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var spec = isAdmin ? new OrderSpecification(orderId) : new OrderSpecification(email, orderId);

        var order = await _unit.Repository<Order>().GetEntityWithSpec(spec);
        if (order == null) return (null, null);

        // This line now correctly finds the GetGateway method
        var gateway = GetGateway(gatewayName);

        return await gateway.CreateRetryPaymentTransactionAsync(orderId);
    }
}
