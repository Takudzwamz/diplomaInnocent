using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Services;

public class OrderFinalizationService : IOrderFinalizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderFinalizationService> _logger;
    private readonly ISiteSettingsService _siteSettings;

    public OrderFinalizationService(IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        IEmailSender emailSender,
        IConfiguration config,
        ISiteSettingsService siteSettings,
        ILogger<OrderFinalizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailSender = emailSender;
        _config = config;
        _logger = logger;
        _siteSettings = siteSettings;
    }

    public async Task<Order?> FinalizePaymentAsync(string paymentReference, string? gatewayTransactionId)
    {
        if (string.IsNullOrEmpty(paymentReference))
        {
            _logger.LogError("Webhook 'charge.success' is missing a reference.");
            return null;
        }

        var spec = new OrderWithItemsSpecification(paymentReference);
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null)
        {
            _logger.LogError("CRITICAL: Webhook received for order with PaymentReference {Reference}, but no order was found.", paymentReference);
            return null;
        }

        // Prevent processing the same successful event twice
        if (order.Status == OrderStatus.PaymentReceived)
        {
            _logger.LogInformation("Webhook for Order {OrderId} already processed.", order.Id);
            return order;
        }

        // --- 1. UPDATE COUPON USAGE ---
        if (order.Discount > 0 && !string.IsNullOrEmpty(order.CouponCode))
        {
            var couponSpec = new BaseSpecification<Coupon>(c => c.Code.ToUpper() == order.CouponCode.ToUpper());
            var coupon = await _unitOfWork.Repository<Coupon>().GetEntityWithSpec(couponSpec);
            if (coupon != null)
            {
                coupon.UsageCount++;
                _unitOfWork.Repository<Coupon>().Update(coupon);
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

        // --- 2. DEDUCT STOCK FROM INVENTORY ---
        var productRepo = _unitOfWork.Repository<Product>();
        var variantRepo = _unitOfWork.Repository<ProductVariant>();

        foreach (var item in order.OrderItems)
        {
            if (item.ItemOrdered.ProductVariantId.HasValue)
            {
                var variant = await variantRepo.GetByIdAsync(item.ItemOrdered.ProductVariantId.Value);
                if (variant != null)
                {
                    variant.QuantityInStock = Math.Max(0, variant.QuantityInStock - item.Quantity);
                    variantRepo.Update(variant);
                }
            }
            else
            {
                var product = await productRepo.GetByIdAsync(item.ItemOrdered.ProductId);
                if (product != null)
                {
                    product.QuantityInStock = Math.Max(0, product.QuantityInStock - item.Quantity);
                    productRepo.Update(product);
                }
            }
        }

        // --- 3. UPDATE ORDER STATUS ---
        order.Status = OrderStatus.PaymentReceived;
        order.DeliveryStatus = DeliveryStatus.Processing;
        order.GatewayTransactionId = gatewayTransactionId;
        order.TrackingEvents.Add(new TrackingEvent
        {
            Status = DeliveryStatus.Processing.ToString(),
            Notes = "Order confirmed. Awaiting fulfillment by the store."
        });
        _unitOfWork.Repository<Order>().Update(order);

        var settings = await _siteSettings.GetSettingsAsync();
        var publicUrl = settings.GetValueOrDefault("PublicUrl", "https://localhost:5001");
        var storeName = settings.GetValueOrDefault("StoreName", "Your Store");
        var orderUrl = $"{publicUrl}/Orders/Details/{order.Id}";
        var adminEmail = settings.GetValueOrDefault("AdminNotificationEmail", "sputnikdevs@sputnikdevs.com");
        var adminOrderUrl = $"{publicUrl}/Admin/Orders/Details/{order.Id}";

        // --- 4. SEND CUSTOMER CONFIRMATION EMAIL ---
        var customerSubject = $"Your {storeName} Order Confirmation [#{order.Id}]";
        var customerMessage = BuildCustomerEmailBody(order, orderUrl);
        await _emailSender.SendEmailAsync(order.BuyerEmail, customerSubject, customerMessage.ToString());

        // --- 5. SEND NEW ADMIN NOTIFICATION EMAIL ---
        var adminSubject = $"🎉 New Order Received [#{order.Id}]";
        var adminMessage = BuildAdminEmailBody(order, adminOrderUrl, storeName);
        await _emailSender.SendEmailAsync(adminEmail, adminSubject, adminMessage.ToString());

        // --- 6. SAVE ALL CHANGES ---
        await _unitOfWork.Complete();
        _logger.LogInformation("Order {OrderId} status updated to PaymentReceived and confirmation email sent.", order.Id);

        return order;
    }

    private StringBuilder BuildCustomerEmailBody(Order order, string orderUrl)
    {
        var message = new StringBuilder();
        message.Append($"<h1>Thank you for your order, {order.ShippingAddress.Name}!</h1>");
        message.Append($"<p>We've received your order #{order.Id} and are getting it ready for shipment. You can view its status at any time using the button below.</p>");
        message.Append($"<a href='{orderUrl}' style='display: inline-block; padding: 12px 24px; font-size: 16px; color: #fff; background-color: #0d6efd; text-decoration: none; border-radius: 5px; margin: 20px 0;'>View Your Order</a>");
        message.Append("<h2>Order Summary</h2>");
        message.Append("<table border='0' cellpadding='10' cellspacing='0' style='width: 100%; border-collapse: collapse;'>");
        message.Append("<thead style='background-color: #f8f9fa;'><tr><th style='text-align: left;' colspan='2'>Item</th><th style='text-align: center;'>Quantity</th><th style='text-align: right;'>Price</th></tr></thead>");
        message.Append("<tbody>");

        foreach (var item in order.OrderItems)
        {
            message.Append("<tr>");
            message.Append($"<td style='padding: 10px; border-bottom: 1px solid #dee2e6; width: 70px;'><img src='{item.ItemOrdered.PictureUrl}' alt='{item.ItemOrdered.ProductName}' style='width: 60px; height: 60px; object-fit: cover; border-radius: 5px;'/></td>");
            // --- ADDED VARIANT OPTIONS TO EMAIL ---
            message.Append($"<td style='border-bottom: 1px solid #dee2e6;'>{item.ItemOrdered.ProductName}");
            if (!string.IsNullOrEmpty(item.ItemOrdered.SelectedOptions))
            {
                message.Append($"<br/><small style='color: #6c757d;'>{item.ItemOrdered.SelectedOptions}</small>");
            }
            message.Append("</td>");
            // --- END OF CHANGE ---
            message.Append($"<td style='text-align: center; border-bottom: 1px solid #dee2e6;'>{item.Quantity}</td>");
            message.Append($"<td style='text-align: right; border-bottom: 1px solid #dee2e6;'>{item.Price:C}</td>");
            message.Append("</tr>");
        }

        message.Append("</tbody></table>");
        message.Append($"<p style='text-align: right; margin: 20px 0 0 0;'><strong>Subtotal:</strong> {order.Subtotal:C}</p>");
        if (order.Discount > 0)
        {
            message.Append($"<p style='text-align: right; margin: 5px 0 0 0; color: green;'><strong>Discount ({order.CouponCode}):</strong> -{order.Discount:C}</p>");
        }
        message.Append($"<p style='text-align: right; margin: 5px 0 0 0;'><strong>Shipping:</strong> {order.DeliveryMethod.Price:C}</p>");
        message.Append($"<h3 style='text-align: right; margin: 10px 0 0 0;'><strong>Total:</strong> {order.GetTotal():C}</h3>");
        message.Append("<h3 style='margin-top: 30px;'>Shipping To</h3>");
        if (!string.IsNullOrEmpty(order.ShippingAddress.PhoneNumber))
        {
            message.Append($"<strong>Phone:</strong> {order.ShippingAddress.PhoneNumber}<br/>");
        }
        message.Append($"<strong>Shipping To:</strong><br/>{order.ShippingAddress.ToHtmlString()}</p>");

        if (!string.IsNullOrEmpty(order.ShippingAddress.DeliveryNotes))
        {
            message.Append("<h3 style='margin-top: 20px;'>Delivery Notes</h3>");
            message.Append($"<p style='font-style: italic; background-color: #f8f9fa; padding: 10px; border-radius: 5px;'>\"{order.ShippingAddress.DeliveryNotes}\"</p>");
        }

        return message;
    }

    // --- NEW METHOD TO BUILD THE ADMIN EMAIL ---
    private StringBuilder BuildAdminEmailBody(Order order, string adminOrderUrl, string storeName)
    {
        var message = new StringBuilder();
        message.Append($"<h1>You've received a new order!</h1>");
        message.Append($"<p>Order #{order.Id} for <strong>{order.GetTotal():C}</strong> was just placed on {storeName}.</p>");
        message.Append($"<a href='{adminOrderUrl}' style='display: inline-block; padding: 12px 24px; font-size: 16px; color: #fff; background-color: #0d6efd; text-decoration: none; border-radius: 5px; margin: 20px 0;'>View in Admin Panel</a>");

        message.Append("<h2>Customer Details</h2>");
        message.Append($"<p><strong>Email:</strong> {order.BuyerEmail}<br/>");

        if (!string.IsNullOrEmpty(order.ShippingAddress.PhoneNumber))
        {
            message.Append($"<strong>Phone:</strong> {order.ShippingAddress.PhoneNumber}<br/>");
        }
        message.Append($"<strong>Shipping To:</strong><br/>{order.ShippingAddress.ToHtmlString()}</p>");

        if (!string.IsNullOrEmpty(order.ShippingAddress.DeliveryNotes))
        {
            message.Append("<h3 style='margin-top: 20px;'>Delivery Notes</h3>");
            message.Append($"<p style='font-style: italic; background-color: #f8f9fa; padding: 10px; border-radius: 5px;'>\"{order.ShippingAddress.DeliveryNotes}\"</p>");
        }

        message.Append("<h2>Items Ordered</h2>");
        message.Append("<table border='0' cellpadding='10' cellspacing='0' style='width: 100%; border-collapse: collapse;'>");
        message.Append("<thead style='background-color: #f8f9fa;'><tr><th style='text-align: left;' colspan='2'>Item</th><th style='text-align: center;'>Quantity</th><th style='text-align: right;'>Price</th></tr></thead>");
        message.Append("<tbody>");

        foreach (var item in order.OrderItems)
        {
            message.Append("<tr>");
            message.Append($"<td style='padding: 10px; border-bottom: 1px solid #dee2e6; width: 70px;'><img src='{item.ItemOrdered.PictureUrl}' alt='{item.ItemOrdered.ProductName}' style='width: 60px; height: 60px; object-fit: cover; border-radius: 5px;'/></td>");
            // --- ADDED VARIANT OPTIONS TO EMAIL ---
            message.Append($"<td style='border-bottom: 1px solid #dee2e6;'>{item.ItemOrdered.ProductName}");
            if (!string.IsNullOrEmpty(item.ItemOrdered.SelectedOptions))
            {
                message.Append($"<br/><small style='color: #6c757d;'>{item.ItemOrdered.SelectedOptions}</small>");
            }
            message.Append("</td>");
            // --- END OF CHANGE ---
            message.Append($"<td style='text-align: center; border-bottom: 1px solid #dee2e6;'>{item.Quantity}</td>");
            message.Append($"<td style='text-align: right; border-bottom: 1px solid #dee2e6;'>{item.Price:C}</td>");
            message.Append("</tr>");
        }

        message.Append("</tbody></table>");
        message.Append($"<p style='text-align: right; margin: 20px 0 0 0;'><strong>Subtotal:</strong> {order.Subtotal:C}</p>");
        if (order.Discount > 0)
        {
            message.Append($"<p style='text-align: right; margin: 5px 0 0 0; color: green;'><strong>Discount ({order.CouponCode}):</strong> -{order.Discount:C}</p>");
        }
        message.Append($"<p style='text-align: right; margin: 5px 0 0 0;'><strong>Shipping:</strong> {order.DeliveryMethod.Price:C}</p>");
        message.Append($"<h3 style='text-align: right; margin: 10px 0 0 0;'><strong>Total:</strong> {order.GetTotal():C}</h3>");

        return message;
    }
}