using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PayStack.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class PaystackGatewayService : IPaymentGateway
{
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unit;
    // private readonly PayStackApi? _paystackApi;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _config;
    /*  private readonly UserManager<AppUser> _userManager;
     private readonly IEmailSender _emailSender; */
    private readonly ISiteSettingsService _siteSettings;
    private readonly IOrderFinalizationService _orderFinalizer;
    private readonly ILogger<PaystackGatewayService> _logger;
    // private bool _isConfigured = false;
    // private string _siteMode = "Test";
    public string GatewayName => "Paystack";


    public PaystackGatewayService(IConfiguration config,
        ICartService cartService,
        IUnitOfWork unit,
        IHttpContextAccessor httpContextAccessor,
        ISiteSettingsService siteSettings,
        // UserManager<AppUser> userManager,
        // IEmailSender emailSender,
        IOrderFinalizationService orderFinalizer,
        ILogger<PaystackGatewayService> logger)
    {
        _config = config;
        _orderFinalizer = orderFinalizer;
        _cartService = cartService;
        _unit = unit;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _siteSettings = siteSettings;

        
    }


    // --- YOU MUST ALSO FIX THIS METHOD ---
    public async Task<(string? authorizationUrl, Dictionary<string, string>? postData)> CreatePaymentTransactionAsync(Order order)
    {
        // 1. Get CURRENT settings
        var settings = await _siteSettings.GetSettingsAsync();
        var currentSiteMode = settings.GetValueOrDefault("Payment_SiteMode", "Test");

        var secretKey = currentSiteMode == "Live"
            ? settings.GetValueOrDefault("Paystack_Live_SecretKey")
            : settings.GetValueOrDefault("Paystack_Test_SecretKey");

        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogError("Paystack {Mode} Secret Key is not configured.", currentSiteMode);
            throw new Exception("Paystack gateway is not configured.");
        }

        // 2. Create the API client just-in-time
        var paystackApi = new PayStackApi(secretKey);

        var publicUrl = settings.GetValueOrDefault("PublicUrl") ?? _config["PublicUrl"] ?? "https://localhost:5001";
        var totalInKobo = (long)(order.GetTotal() * 100);
        var reference = order.PaymentReference;
        var email = order.BuyerEmail;
        var callbackUrl = $"{publicUrl}/Orders/Confirmation?reference={reference}";

        var transactionRequest = new TransactionInitializeRequest
        {
            AmountInKobo = (int)totalInKobo,
            Email = email,
            Reference = reference,
            Currency = "ZAR",
            CallbackUrl = callbackUrl
        };

        var transactionResponse = paystackApi.Transactions.Initialize(transactionRequest);
        if (!transactionResponse.Status)
        {
            _logger.LogError("Paystack transaction failed: {Message}", transactionResponse.Message);
            return (null, null);
        }

        return (transactionResponse.Data.AuthorizationUrl, null);
    }

    // --- THIS IS THE CRITICAL FIX ---
    public async Task<Order?> HandleWebhookAsync(string jsonPayload, string signature)
    {
        // 1. Get CURRENT settings
        var settings = await _siteSettings.GetSettingsAsync();
        var currentSiteMode = settings.GetValueOrDefault("Payment_SiteMode", "Test");

        // 2. Get the correct key based on CURRENT mode
        var paystackSecret = currentSiteMode == "Live"
            ? settings.GetValueOrDefault("Paystack_Live_SecretKey")
            : settings.GetValueOrDefault("Paystack_Test_SecretKey");

        if (string.IsNullOrEmpty(paystackSecret))
        {
            _logger.LogError("Paystack webhook received but secret key for {Mode} mode is not configured. Aborting.", currentSiteMode);
            return null;
        }

        // 3. Perform the signature check
        var computedHash = ComputeSha512Hash(jsonPayload, paystackSecret);
        if (computedHash != signature)
        {
            _logger.LogWarning("Paystack webhook signature verification failed. Key used was for {Mode} mode.", currentSiteMode);
            return null; // This is what's happening to you
        }

        // 4. Continue with processing
        var paystackEvent = JObject.Parse(jsonPayload);
        var eventType = paystackEvent["event"]?.ToString();
        var eventData = paystackEvent["data"];

        if (eventType == "charge.success" && eventData != null)
        {
            var reference = eventData["reference"]?.ToString();
            var gatewayId = eventData["id"]?.ToString();
            return await _orderFinalizer.FinalizePaymentAsync(reference!, gatewayId);
        }

        _logger.LogInformation("Received a Paystack event that was not 'charge.success': {EventType}", eventType);
        return null;
    }


    public async Task<string> RefundOrderAsync(Order order)
    {
        // --- THIS IS THE FIX ---
        // 1. Get CURRENT settings just-in-time
        var settings = await _siteSettings.GetSettingsAsync();
        var currentSiteMode = settings.GetValueOrDefault("Payment_SiteMode", "Test");
        var secretKey = currentSiteMode == "Live"
            ? settings.GetValueOrDefault("Paystack_Live_SecretKey")
            : settings.GetValueOrDefault("Paystack_Test_SecretKey");

        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogError("Paystack (Refund) {Mode} Secret Key is not configured.", currentSiteMode);
            throw new Exception("Paystack gateway is not configured.");
        }

        // 2. Create the API client just-in-time
        var paystackApi = new PayStackApi(secretKey);
        // --- END OF FIX ---

        // Paystack uses our internal PaymentReference
        string transactionReference = order.PaymentReference;

        // 3. Use the new, local paystackApi instance
        var refundResponse = paystackApi.Post<ApiResponse<object>, object>("/refund",
            new { transaction = transactionReference }
        );

        var resultMessage = refundResponse.Status
            ? "Refund initiated successfully"
            : $"Refund failed: {refundResponse.Message}";

        return resultMessage; // Just return the string directly
    }

    // --- Helper methods moved from old service ---
    private async Task ValidateCartItemsInCartAsync(ShoppingCart cart)
    {
        foreach (var item in cart.Items)
        {
            decimal currentPrice;
            if (item.ProductVariantId.HasValue)
            {
                // It's a variant, get the variant's price
                var variant = await _unit.Repository<ProductVariant>().GetByIdAsync(item.ProductVariantId.Value)
                    ?? throw new Exception($"Product variant with ID {item.ProductVariantId.Value} not found.");
                currentPrice = variant.Price;
            }
            else
            {
                // It's a simple product, get the product's price
                var productItem = await _unit.Repository<Product>().GetByIdAsync(item.ProductId)
                    ?? throw new Exception($"Product with ID {item.ProductId} not found.");
                currentPrice = productItem.Price;
            }

            // If the price in the cart doesn't match the database, update it
            if (item.Price != currentPrice)
            {
                item.Price = currentPrice;
            }
        }
    }
    private static string ComputeSha512Hash(string text, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var textBytes = Encoding.UTF8.GetBytes(text);
        using var hash = new HMACSHA512(keyBytes);
        var hashBytes = hash.ComputeHash(textBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public async Task<(Order? Order, string? AuthorizationUrl)> CreateRetryPaymentTransactionAsync(int orderId)
    {
        var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            throw new Exception("User email not found.");
        }

        var spec = new OrderSpecification(email, orderId);
        var order = await _unit.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null) return (null, null);
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.PaymentFailed)
        {
            return (null, null);
        }

        // Check stock for all items
        var productRepo = _unit.Repository<Product>();
        var variantRepo = _unit.Repository<ProductVariant>();
        foreach (var item in order.OrderItems)
        {
            int currentStock = 0;
            if (item.ItemOrdered.ProductVariantId.HasValue)
            {
                var variant = await variantRepo.GetByIdAsync(item.ItemOrdered.ProductVariantId.Value);
                currentStock = variant?.QuantityInStock ?? 0;
            }
            else
            {
                var product = await productRepo.GetByIdAsync(item.ItemOrdered.ProductId);
                currentStock = product?.QuantityInStock ?? 0;
            }
            if (currentStock < item.Quantity)
            {
                throw new Exception($"Sorry, the product '{item.ItemOrdered.ProductName}' is no longer in stock.");
            }
        }

        // --- THIS IS THE FIX ---
        // 1. Get CURRENT settings just-in-time
        var settings = await _siteSettings.GetSettingsAsync();
        var currentSiteMode = settings.GetValueOrDefault("Payment_SiteMode", "Test");
        var secretKey = currentSiteMode == "Live"
            ? settings.GetValueOrDefault("Paystack_Live_SecretKey")
            : settings.GetValueOrDefault("Paystack_Test_SecretKey");

        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogError("Paystack (Retry) {Mode} Secret Key is not configured.", currentSiteMode);
            throw new Exception("Paystack gateway is not configured.");
        }

        // 2. Create the API client just-in-time
        var paystackApi = new PayStackApi(secretKey);
        // --- END OF FIX ---

        var totalInKobo = (long)(order.GetTotal() * 100);
        var newReference = Guid.NewGuid().ToString(); // Generate a NEW reference

        // This part of your original code was already correct
        var publicUrl = settings.GetValueOrDefault("PublicUrl") ?? _config["PublicUrl"] ?? "https://localhost:5001";
        var callbackUrl = $"{publicUrl}/Orders/Confirmation?reference={newReference}";

        var transactionRequest = new TransactionInitializeRequest
        {
            AmountInKobo = (int)totalInKobo,
            Email = email,
            Reference = newReference,
            Currency = "ZAR",
            CallbackUrl = callbackUrl
        };

        // 3. Use the new, local paystackApi instance
        var transactionResponse = paystackApi.Transactions.Initialize(transactionRequest);

        if (!transactionResponse.Status)
        {
            throw new Exception($"Paystack transaction failed: {transactionResponse.Message}");
        }

        order.PaymentReference = transactionResponse.Data.Reference;
        _unit.Repository<Order>().Update(order);
        await _unit.Complete();

        return (order, transactionResponse.Data.AuthorizationUrl);
    }

}