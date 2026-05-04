/* using System.Security.Cryptography;
using System.Text;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace StorefrontRazor.Pages.Webhooks;

// This tells ASP.NET Core not to require an anti-forgery token for this page,
// as it will be called by an external service.
[IgnoreAntiforgeryToken]
public class PaystackModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly ILogger<PaystackModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<AppUser> _userManager;

    public PaystackModel(IUnitOfWork unitOfWork, IConfiguration config, ILogger<PaystackModel> logger, UserManager<AppUser> userManager, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _config = config;
        _logger = logger;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    // This handler will be triggered by POST requests to /Webhooks/Paystack
    public async Task<IActionResult> OnPostAsync()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var paystackSecret = _config["PaystackSettings:SecretKey"]!;

        // 1. Verify the webhook signature to ensure it's from Paystack
        var signature = Request.Headers["x-paystack-signature"].ToString();
        var computedHash = ComputeSha512Hash(json, paystackSecret);
        if (computedHash != signature)
        {
            _logger.LogWarning("Paystack webhook signature verification failed.");
            return Unauthorized();
        }

        // 2. Process the event
        var paystackEvent = JObject.Parse(json);
        var eventType = paystackEvent["event"]?.ToString();
        var eventData = paystackEvent["data"];

        if (eventType == "charge.success" && eventData != null)
        {
            await HandleChargeSuccess(eventData);
        }
        else
        {
            _logger.LogInformation("Received a Paystack event that was not 'charge.success': {EventType}", eventType);
        }

        // Always return a 200 OK to let Paystack know we received the event.
        return new OkResult();
    }



    private async Task HandleChargeSuccess(JToken eventData)
    {
        var reference = eventData["reference"]?.ToString();
        if (string.IsNullOrEmpty(reference))
        {
            _logger.LogError("Paystack webhook 'charge.success' is missing a reference.");
            return;
        }

        // You need to include the OrderItems and the Product details within them.
        var spec = new OrderWithItemsSpecification(reference);
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null)
        {
            _logger.LogError("CRITICAL: Webhook received for order with PaymentReference {Reference}, but no order was found.", reference);
            return;
        }

        // Prevent processing the same successful event twice
        if (order.Status == OrderStatus.PaymentReceived) return;

        // If the order had a discount, find the coupon and increment its usage count.
        if (order.Discount > 0 && !string.IsNullOrEmpty(order.CouponCode)) // Note: We need to add CouponCode to the Order entity
        {
            var couponSpec = new BaseSpecification<Coupon>(c => c.Code.ToUpper() == order.CouponCode.ToUpper());
            var coupon = await _unitOfWork.Repository<Coupon>().GetEntityWithSpec(couponSpec);
            if (coupon != null)
            {
                coupon.UsageCount++;
                _unitOfWork.Repository<Coupon>().Update(coupon);

                // If the coupon was limited per customer, add a record to our tracking table.
                if (coupon.LimitOnePerCustomer)
                {
                    var user = await _userManager.FindByEmailAsync(order.BuyerEmail);
                    if (user != null)
                    {
                        var usageRecord = new CouponUsage { CouponId = coupon.Id, AppUserId = user.Id };
                        _unitOfWork.Repository<CouponUsage>().Add(usageRecord);
                    }
                }
            }
        }

        // 1. DEDUCT STOCK FROM INVENTORY
        var productRepo = _unitOfWork.Repository<Product>();
        foreach (var item in order.OrderItems)
        {
            var product = await productRepo.GetByIdAsync(item.ItemOrdered.ProductId);
            if (product != null)
            {
                // Ensure stock doesn't go below zero
                product.QuantityInStock = Math.Max(0, product.QuantityInStock - item.Quantity);
                productRepo.Update(product);
            }
            else
            {
                _logger.LogWarning("Product with ID {ProductId} not found for stock deduction in Order {OrderId}.", item.ItemOrdered.ProductId, order.Id);
            }
        }

        // 2. UPDATE ORDER STATUS
        order.Status = OrderStatus.PaymentReceived;
        order.DeliveryStatus = DeliveryStatus.Processing;

        var initialTrackingEvent = new TrackingEvent
        {
            Status = DeliveryStatus.Processing.ToString(),
            Notes = "Заказ подтверждён. Ожидается выполнение магазином."
        };
        order.TrackingEvents.Add(initialTrackingEvent);

        _unitOfWork.Repository<Order>().Update(order);

        var subject = $"Подтверждение заказа [№{order.Id}]";
        var publicUrl = _config["PublicUrl"] ?? "https://localhost:5001";
        var orderUrl = $"{publicUrl}/Orders/Details/{order.Id}";

        var message = new StringBuilder();
        message.Append($"<h1>Спасибо за ваш заказ, {order.ShippingAddress.Name}!</h1>");
        message.Append($"<p>Мы получили ваш заказ №{order.Id} и готовим его к отправке. Вы можете отслеживать статус в любое время, нажав на кнопку ниже.</p>");
        message.Append($"<a href='{orderUrl}' style='display: inline-block; padding: 12px 24px; font-size: 16px; color: #fff; background-color: #0d6efd; text-decoration: none; border-radius: 5px; margin: 20px 0;'>Просмотреть заказ</a>");
        message.Append("<h2>Состав заказа</h2>");
        message.Append("<table border='0' cellpadding='10' cellspacing='0' style='width: 100%; border-collapse: collapse;'>");
        // --- START: MODIFIED TABLE HEADER ---
        message.Append("<thead style='background-color: #f8f9fa;'><tr><th style='text-align: left;' colspan='2'>Товар</th><th style='text-align: center;'>Кол-во</th><th style='text-align: right;'>Цена</th></tr></thead>");
        // --- END: MODIFIED TABLE HEADER ---
        message.Append("<tbody>");

        foreach (var item in order.OrderItems)
        {
            message.Append("<tr>");
            // Image Cell
            message.Append($"<td style='padding: 10px; border-bottom: 1px solid #dee2e6; width: 70px;'><img src='{item.ItemOrdered.PictureUrl}' alt='{item.ItemOrdered.ProductName}' style='width: 60px; height: 60px; object-fit: cover; border-radius: 5px;'/></td>");
            // Product Name Cell
            message.Append($"<td style='border-bottom: 1px solid #dee2e6;'>{item.ItemOrdered.ProductName}</td>");
            // Quantity Cell
            message.Append($"<td style='text-align: center; border-bottom: 1px solid #dee2e6;'>{item.Quantity}</td>");
            // Price Cell
            message.Append($"<td style='text-align: right; border-bottom: 1px solid #dee2e6;'>{item.Price:C}</td>");
            message.Append("</tr>");
        }

        message.Append("</tbody></table>");
        message.Append($"<p style='text-align: right; margin: 20px 0 0 0;'><strong>Подитог:</strong> {order.Subtotal:C}</p>");
        if (order.Discount > 0)
        {
            message.Append($"<p style='text-align: right; margin: 5px 0 0 0; color: green;'><strong>Скидка ({order.CouponCode}):</strong> -{order.Discount:C}</p>");
        }
        message.Append($"<p style='text-align: right; margin: 5px 0 0 0;'><strong>Доставка:</strong> {order.DeliveryMethod.Price:C}</p>");
        message.Append($"<h3 style='text-align: right; margin: 10px 0 0 0;'><strong>Итого:</strong> {order.GetTotal():C}</h3>");

        message.Append("<h3 style='margin-top: 30px;'>Адрес доставки</h3>");
        message.Append($"<p style='line-height: 1.6;'>");
        message.Append($"{order.ShippingAddress.Name} {order.ShippingAddress.LastName}<br/>");
        message.Append($"{order.ShippingAddress.Line1}<br/>");
        if (!string.IsNullOrEmpty(order.ShippingAddress.Line2))
        {
            message.Append($"{order.ShippingAddress.Line2}<br/>");
        }
        message.Append($"{order.ShippingAddress.City}, {order.ShippingAddress.State} {order.ShippingAddress.PostalCode}<br/>");
        message.Append($"{order.ShippingAddress.Country}");
        message.Append("</p>");

        await _emailSender.SendEmailAsync(order.BuyerEmail, subject, message.ToString());

        // 3. SAVE ALL CHANGES IN A SINGLE TRANSACTION
        await _unitOfWork.Complete();
        _logger.LogInformation("Order {OrderId} status updated to PaymentReceived and confirmation email sent.", order.Id);
    }
    private static string ComputeSha512Hash(string text, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var textBytes = Encoding.UTF8.GetBytes(text);
        using var hash = new HMACSHA512(keyBytes);
        var hashBytes = hash.ComputeHash(textBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
 */



 using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Webhooks;

[IgnoreAntiforgeryToken]
public class PaystackModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaystackModel> _logger;

    public PaystackModel(IPaymentService paymentService, ILogger<PaystackModel> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["x-paystack-signature"].ToString();

        try
        {
            var order = await _paymentService.HandleWebhookAsync("Paystack", json, signature);

            if (order == null)
            {
                _logger.LogWarning("Paystack webhook processed, but no order was updated (e.g., signature fail or not 'charge.success').");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Paystack webhook.");
            return new StatusCodeResult(500); // Internal server error
        }

        return new OkResult(); // Always return 200 OK to Paystack
    }
}