using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PayFast;
using PayFast.AspNetCore;
using System.Security.Claims;
using System.Web; // Required for HttpUtility
using System.Collections.Specialized; // Required for NameValueCollection
using System.Reflection; // Required for Reflection
using System.Net;
using System.Security.Cryptography; // For MD5 hashing
using System.Text; // For StringBuilder
using System.Linq;
using Core.Entities;
using System.Text.Json;
// using Infrastructure.Services.PaymentHelpers; // For .ToDictionary()
using System.Net.Http; // For HttpRequestMessage
using System.Net.Http.Headers; // For "application/json"

namespace Infrastructure.Services;

public class PayFastGatewayService : IPaymentGateway
{
    private readonly IUnitOfWork _unit;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISiteSettingsService _siteSettings;
    private readonly IOrderFinalizationService _orderFinalizer;
    private readonly ILogger<PayFastGatewayService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly bool _isConfigured = false;
    private readonly string _siteMode = "Test";
    private readonly string _merchantId = string.Empty;
    private readonly string _merchantKey = string.Empty;
    private readonly string _passphrase = string.Empty;

    public string GatewayName => "PayFast";

    public PayFastGatewayService(
        IUnitOfWork unit,
        IHttpContextAccessor httpContextAccessor,
        ISiteSettingsService siteSettings,
        IOrderFinalizationService orderFinalizer,
        ILogger<PayFastGatewayService> logger,
        IHttpClientFactory httpClientFactory) // Inject HttpClientFactory for ITN validation
    {
        _unit = unit;
        _httpContextAccessor = httpContextAccessor;
        _siteSettings = siteSettings;
        _orderFinalizer = orderFinalizer;
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        var settings = _siteSettings.GetSettingsAsync().Result;
        _siteMode = settings.GetValueOrDefault("Payment_SiteMode", "Test") ?? "Test";

        if (_siteMode == "Live")
        {
            _merchantId = settings.GetValueOrDefault("PayFast_Live_MerchantId") ?? string.Empty;
            _merchantKey = settings.GetValueOrDefault("PayFast_Live_MerchantKey") ?? string.Empty;
            _passphrase = settings.GetValueOrDefault("PayFast_Live_Passphrase") ?? string.Empty;
        }
        else
        {
            _merchantId = settings.GetValueOrDefault("PayFast_Test_MerchantId") ?? string.Empty;
            _merchantKey = settings.GetValueOrDefault("PayFast_Test_MerchantKey") ?? string.Empty;
            _passphrase = settings.GetValueOrDefault("PayFast_Test_Passphrase") ?? string.Empty;
        }

        if (string.IsNullOrEmpty(_merchantId) ||
            string.IsNullOrEmpty(_merchantKey) ||
            string.IsNullOrEmpty(_passphrase))
        {
            _logger.LogWarning("PayFast {Mode} settings are not fully configured.", _siteMode);
            _isConfigured = false;
        }
        else
        {
            _isConfigured = true;
        }
    }


    /// <summary>
    /// Creates a payment transaction by generating the form data to be POSTed to PayFast.
    /// </summary>
    public async Task<(string? authorizationUrl, Dictionary<string, string>? postData)> CreatePaymentTransactionAsync(Order order)
    {
        if (!_isConfigured) throw new Exception("PayFast gateway is not configured.");

        var settings = await _siteSettings.GetSettingsAsync();
        var publicUrl = settings.GetValueOrDefault("PublicUrl") ?? "https://localhost:5001";
        var storeName = settings.GetValueOrDefault("StoreName", "Your Store");

        // 1. *** THE FIX ***
        // Re-order the list to match the documentation's "attributes" order.
        var postDataList = new List<KeyValuePair<string, string>>
        {
            // Merchant Details (Order matters!)
            new("merchant_id", _merchantId),
            new("merchant_key", _merchantKey),
            new("return_url", $"{publicUrl}/Orders/Confirmation?reference={order.PaymentReference}".Trim()),
            new("cancel_url", $"{publicUrl}/Orders/Failed?reference={order.PaymentReference}".Trim()),
            new("notify_url", $"{publicUrl}/webhook/payfast".Trim()),

            // Buyer Details (This order is from Doc 1's example)
            new("name_first", order.ShippingAddress.Name.Trim()),
            new("name_last", order.ShippingAddress.LastName.Trim()),
            new("email_address", order.BuyerEmail.Trim()),

            // Transaction Details
            new("m_payment_id", order.PaymentReference.Trim()),
            new("amount", order.GetTotal().ToString("F2", System.Globalization.CultureInfo.InvariantCulture)),
            new("item_name", $"Order #{order.Id} from {storeName}".Trim()),
            new("item_description", $"Payment for Order #{order.Id}".Trim()),
            
            // Transaction Options (From old code)
            new("email_confirmation", "0"),
            new("confirmation_address", order.BuyerEmail.Trim())
        };

        // 2. Generate the signature from the *ordered list*
        var signature = GeneratePayFastSignature(postDataList, _passphrase);

        // 3. Convert the List to a Dictionary for the Razor Page
        var postDataDict = postDataList.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // 4. Add the signature to the Dictionary
        postDataDict.Add("signature", signature);

        // 5. Get the processing URL
        var processUrl = _siteMode == "Live"
            ? "https://www.payfast.co.za/eng/process"
            : "https://sandbox.payfast.co.za/eng/process";

        // 6. Log the data being sent
        _logger.LogInformation("--- Generating PayFast Post Data ---");
        foreach (var (key, value) in postDataDict)
        {
            if (!key.Equals("signature", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("PayFast Data: {Key} = {Value}", key, value);
            }
        }
        _logger.LogInformation("--- PayFast Post Data Generation Complete ---");


        // 7. Return the URL and the completed dictionary
        return (processUrl, postDataDict);
    }
    /// <summary>
    /// A helper to precisely mimic PHP's 'urlencode' function, which uses
    /// uppercase hex escapes and '+' for spaces.
    /// </summary>
    private string PhpUrlEncode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        // 1. Use Uri.EscapeDataString for RFC 3986-compliant encoding (uppercase hex, %20 for space)
        string encoded = Uri.EscapeDataString(value.Trim());

        // 2. Replace %20 with '+' to match PHP's urlencode 
        return encoded.Replace("%20", "+");
    }


    /// <summary>
    /// Manually generates a PayFast signature.
    /// </summary>
    private string GeneratePayFastSignature(List<KeyValuePair<string, string>> postDataList, string passphrase)
    {
        var stringBuilder = new StringBuilder();

        // 1. Iterate list, respecting insertion order
        foreach (var (key, value) in postDataList)
        {
            // *** THE FIX: Only add non-blank, non-null values ***
            // This matches the Node.js example: if (data[key] !== "")
            if (!string.IsNullOrEmpty(value))
            {
                stringBuilder.Append($"{key}={PhpUrlEncode(value)}&");
            }
        }

        // 2. Remove the last trailing '&'
        stringBuilder.Length--;

        // 3. Append the passphrase *at the end*
        stringBuilder.Append($"&passphrase={PhpUrlEncode(passphrase)}");

        // 4. Get the final string
        string finalString = stringBuilder.ToString();

        // 5. Log the string
        _logger.LogInformation("--- PayFast Signature String (v12 - Filtered Blanks) ---");
        _logger.LogInformation(finalString);
        _logger.LogInformation("--- End Signature String ---");

        // 6. Create MD5 hash
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(finalString));

        // 7. Convert hash to lowercase hexadecimal string
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }


    /// <summary>
    /// Generates a signature for the PayFast REST API (e.g., Refunds)
    /// which requires ALPHABETICAL sorting of all headers and body params.
    /// </summary>
    private string GenerateApiSignature(Dictionary<string, string> headers, Dictionary<string, string> body, string passphrase)
    {
        // 1. Use a SortedDictionary to automatically sort all parameters alphabetically
        var sortedParams = new SortedDictionary<string, string>();

        // 2. Add all header and body params
        foreach (var (key, value) in headers)
        {
            if (!string.IsNullOrEmpty(value))
                sortedParams.Add(key, value.Trim());
        }
        foreach (var (key, value) in body)
        {
            if (!string.IsNullOrEmpty(value))
                sortedParams.Add(key, value.Trim());
        }

        // 3. Add the passphrase
        sortedParams.Add("passphrase", passphrase);

        // 4. Build the signature string
        var stringBuilder = new StringBuilder();
        foreach (var (key, value) in sortedParams)
        {
            // Use the same robust encoder we used for payments
            stringBuilder.Append($"{key}={PhpUrlEncode(value)}&");
        }

        // 5. Remove the last trailing '&'
        stringBuilder.Length--;
        string finalString = stringBuilder.ToString();

        // 6. Log for debugging
        _logger.LogInformation("--- PayFast API Signature String ---");
        _logger.LogInformation(finalString);
        _logger.LogInformation("--- End Signature String ---");

        // 7. Create MD5 hash (using UTF-8 is the safe standard)
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(finalString));
        
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }



    /// <summary>
    /// Handles the incoming ITN (webhook) from PayFast.
    /// </summary>
    public async Task<Order?> HandleWebhookAsync(string payload, string signature)
    {
        if (!_isConfigured)
        {
            _logger.LogError("PayFast ITN received but gateway is not configured.");
            return null;
        }

        // 1. Parse the incoming form payload
        var itnData = HttpUtility.ParseQueryString(payload);

        // 2. Instantiate PayFastNotify and manually map properties
        var notify = new PayFastNotify();
        MapNameValueCollectionToPayFastNotify(itnData, notify);

        // 3. *** FIX ***: Set up PayFast settings, *including the validation URL*
        var validationUrl = _siteMode == "Live"
            ? "https://www.payfast.co.za/eng/query/validate"
            : "https://sandbox.payfast.co.za/eng/query/validate";

        var payFastSettings = new PayFastSettings
        {
            MerchantId = _merchantId,
            MerchantKey = _merchantKey,
            PassPhrase = _passphrase,
            ValidateUrl = validationUrl // This is required by the internal ValidateData method
        };

        // 4. Get the remote IP Address
        var remoteIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
        if (remoteIpAddress == null)
        {
            _logger.LogError("PayFast ITN: Could not determine remote IP address. Aborting validation.");
            return null;
        }

        // 5. *** FIX for CS1061 'IsIPv6Loopback' ***
        // Use .Equals() against the static property instead
        if (remoteIpAddress.Equals(IPAddress.IPv6Loopback))
        {
            remoteIpAddress = IPAddress.Loopback;
        }

        // 6. Instantiate the validator
        var validator = new PayFastValidator(payFastSettings, notify, remoteIpAddress);

        // 7. *** FIX for CS1061 'ValidateAsync' ***
        // Call the three separate validation methods provided by the library
        var isMerchantIdValid = validator.ValidateMerchantId();
        var isSourceIpValid = await validator.ValidateSourceIp();
        var isDataValid = await validator.ValidateData(); // This will use the ValidateUrl from settings

        // 8. Check all three results
        if (!isMerchantIdValid)
        {
            _logger.LogWarning("PayFast ITN validation failed: Merchant ID mismatch.");
            return null;
        }
        if (!isSourceIpValid)
        {
            _logger.LogWarning("PayFast ITN validation failed: Source IP address invalid.");
            return null;
        }
        if (!isDataValid)
        {
            // Note: The internal ValidateDataAsync handles the HttpClient creation and posting.
            _logger.LogWarning("PayFast ITN validation failed: Data validation (signature) failed.");
            return null;
        }

        // 9. ITN is fully valid, process the payment status
        _logger.LogInformation("PayFast ITN validation successful.");
        var paymentStatus = notify.payment_status;
        var ourReference = notify.m_payment_id;
        var payfastReference = notify.pf_payment_id;

        if (paymentStatus == "COMPLETE")
        {
            _logger.LogInformation("PayFast ITN received for 'COMPLETE' order. Reference: {Reference}", ourReference);
            // Call the shared finalization service
            return await _orderFinalizer.FinalizePaymentAsync(ourReference, payfastReference);
        }
        else if (paymentStatus == "FAILED")
        {
            _logger.LogWarning("PayFast ITN received for 'FAILED' order. Reference: {Reference}", ourReference);
            var spec = new OrderWithItemsSpecification(ourReference);
            var order = await _unit.Repository<Order>().GetEntityWithSpec(spec);
            if (order != null && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.PaymentFailed;
                _unit.Repository<Order>().Update(order);
                await _unit.Complete();
            }
            return null;
        }
        else
        {
            _logger.LogInformation("PayFast ITN received with status: {Status}. Ignoring.", paymentStatus);
            return null;
        }
    }

    /// <summary>
    /// Issues a refund for the specified order via the specified gateway.
    /// </summary>
    public async Task<string> RefundOrderAsync(Order order)
    {
        // 1. Get settings
        var settings = await _siteSettings.GetSettingsAsync();
        var siteMode = settings.GetValueOrDefault("Payment_SiteMode", "Test");
        string? merchantId, passphrase;

        if (siteMode == "Live")
        {
            merchantId = settings.GetValueOrDefault("PayFast_Live_MerchantId");
            passphrase = settings.GetValueOrDefault("PayFast_Live_Passphrase");
        }
        else
        {
            merchantId = settings.GetValueOrDefault("PayFast_Test_MerchantId");
            passphrase = settings.GetValueOrDefault("PayFast_Test_Passphrase");
        }

        if (string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(passphrase))
        {
            return "PayFast is not configured for refunds.";
        }

        // 2. Get the correct ID. PayFast needs its ID, not ours.
        if (string.IsNullOrEmpty(order.GatewayTransactionId))
        {
            return "Refund failed: PayFast Transaction ID not found on order.";
        }
        var pfPaymentId = order.GatewayTransactionId;
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        var amountInCents = ((long)(order.GetTotal() * 100)).ToString();

        // 3. Build Header and Body for Refund API
        var headers = new Dictionary<string, string>
        {
            { "merchant-id", merchantId },
            { "version", "v1" },
            { "timestamp", timestamp }
        };
        
        var body = new Dictionary<string, string>
        {
            { "amount", amountInCents },
            { "reason", "Customer refund request (via Admin Panel)" },
            { "notify_buyer", "1" }
            // This attempts a "source" (e.g., Credit Card) refund.
            // For a BANK_PAYOUT, the API requires bank details, which is a more complex UI.
        };

        // 4. Generate Signature using our new API-specific helper
        var signature = GenerateApiSignature(headers, body, passphrase);
        headers.Add("signature", signature);

        // 5. Call the API
        try
        {
            var client = _httpClientFactory.CreateClient();
            // The Sandbox for the REST API is the LIVE URL with "?testing=true"
            string apiUrl;
            if (siteMode == "Test")
            {
                // We add "?testing=true" to the production API URL
                apiUrl = $"https://api.payfast.co.za/refunds/{pfPaymentId}?testing=true";
            }
            else
            {
                apiUrl = $"https://api.payfast.co.za/refunds/{pfPaymentId}";
            }
            
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            foreach (var (key, value) in headers) { request.Headers.Add(key, value); }
            
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("PayFast refund successful for {pfPaymentId}: {Response}", pfPaymentId, responseBody);
                return "Refund initiated successfully";
            }
            
            _logger.LogError("PayFast refund failed: {Response}", responseBody);
            return $"Refund failed: {responseBody}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayFast refund.");
            return "Refund failed due to an internal error.";
        }
    }

    /// <summary>
    /// Retries a failed payment for an existing order.
    /// </summary>
    public async Task<(Order? Order, string? AuthorizationUrl)> CreateRetryPaymentTransactionAsync(int orderId)
    {
        // --- THIS IS THE FIX ---
        // 1. Get the fresh settings *inside* the method, just like in CreatePaymentTransactionAsync
        var settings = await _siteSettings.GetSettingsAsync();
        var siteMode = settings.GetValueOrDefault("Payment_SiteMode", "Test");
        string? merchantId, merchantKey, passphrase;

        if (siteMode == "Live")
        {
            merchantId = settings.GetValueOrDefault("PayFast_Live_MerchantId");
            merchantKey = settings.GetValueOrDefault("PayFast_Live_MerchantKey");
            passphrase = settings.GetValueOrDefault("PayFast_Live_Passphrase");
        }
        else
        {
            merchantId = settings.GetValueOrDefault("PayFast_Test_MerchantId");
            merchantKey = settings.GetValueOrDefault("PayFast_Test_MerchantKey");
            passphrase = settings.GetValueOrDefault("PayFast_Test_Passphrase");
        }

        // 2. Check if the gateway is configured
        if (string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(merchantKey) || string.IsNullOrEmpty(passphrase))
        {
            _logger.LogError("PayFast {Mode} is not fully configured. Cannot retry payment.", siteMode);
            throw new Exception("PayFast gateway is not configured.");
        }
        // --- END OF FIX ---

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

        // Stock check (this logic is correct)
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

        order.PaymentReference = Guid.NewGuid().ToString();
        _unit.Repository<Order>().Update(order);
        await _unit.Complete();

        // We pass the order, not just the ID
        //  await CreatePaymentTransactionAsync(order);

        // For PayFast, we return the order, but the URL is null
        return (order, null);
    }


    #region Private Helpers


    /// <summary>
    /// *** NEW HELPER ***
    /// Manually maps the ITN form data (NameValueCollection) to the PayFastNotify object.
    /// </summary>
    private void MapNameValueCollectionToPayFastNotify(NameValueCollection collection, PayFastNotify notify)
    {
        var properties = notify.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            var value = collection[prop.Name];
            if (value != null)
            {
                try
                {
                    // Attempt to convert to the property's type
                    var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(notify, convertedValue, null);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not map ITN property {PropertyName}", prop.Name);
                }
            }
        }
    }

    #endregion
}