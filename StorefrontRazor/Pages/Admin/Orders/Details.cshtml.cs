using System.Text.Json;
using Core.DTOs;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StorefrontRazor.Pages.Admin.Orders;

public class DetailsModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;

    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public DetailsModel(IUnitOfWork unitOfWork, IPaymentService paymentService, IEmailSender emailSender, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _emailSender = emailSender;
        _config = config;
    }

    public OrderDto Order { get; set; } = default!;
    
    [BindProperty]
    public string NewStatus { get; set; } = string.Empty;
    
    [BindProperty]
    public string Notes { get; set; } = string.Empty;
    
    public SelectList DeliveryStatusOptions { get; } = new(new[] { "Shipped", "OutForDelivery", "Delivered" });
    
    public string StatusNotesJson { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Детали заказа";
        var spec = new OrderSpecification(id);
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null) return NotFound();

        Order = order.ToDto();

        var statusNotes = new Dictionary<string, string>
        {
            { "Shipped", "Ваш заказ отправлен." },
            { "OutForDelivery", "Ваш заказ передан курьеру." },
            { "Delivered", "Ваш заказ успешно доставлен." }
        };
        StatusNotesJson = JsonSerializer.Serialize(statusNotes);
        
        NewStatus = "Shipped";
        Notes = GetDefaultNoteForStatus(NewStatus);
        
        return Page();
    }

    
    public async Task<IActionResult> OnPostUpdateStatusAsync(int id)
    {
        // This spec correctly includes the OrderItems needed for the email
        var spec = new OrderSpecification(id);
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
        if (order == null) return NotFound();

        // 1. Update the order status and add a tracking event
        order.TrackingEvents.Add(new TrackingEvent { Status = NewStatus, Notes = Notes });
        if (Enum.TryParse<DeliveryStatus>(NewStatus, true, out var newDeliveryStatus))
        {
            order.DeliveryStatus = newDeliveryStatus;
        }

        // 2. Compose and send the email notification
        var subject = $"Ваш заказ #{order.Id} — статус: {NewStatus}!";
        var publicUrl = _config["PublicUrl"] ?? "https://localhost:5001"; // Fallback URL
        var orderUrl = $"{publicUrl}/Orders/Details/{order.Id}";

        var message = $@"
            <h1>Здравствуйте, {order.ShippingAddress.Name}!</h1>
            <p>Статус вашего заказа #{order.Id} был обновлён.</p>
            <p><strong>Новый статус:</strong> {NewStatus}</p>
            <p><strong>Комментарий:</strong> {Notes}</p>
            <p>Вы можете просмотреть детали заказа и историю отслеживания, нажав на кнопку ниже:</p>
            <a href='{orderUrl}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #fff; background-color: #0d6efd; text-decoration: none; border-radius: 5px;'>Просмотреть заказ</a>
            <p>Спасибо за покупку!</p>";
            
        await _emailSender.SendEmailAsync(order.BuyerEmail, subject, message);

        // 3. Save the changes to the database
        await _unitOfWork.Complete();
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRefundOrderAsync(int id)
    {
        var spec = new OrderWithItemsSpecification(id);
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null) return NotFound();

        // Check if the order is in a refundable state
        if (order.Status != OrderStatus.PaymentReceived)
        {
            TempData["ErrorMessage"] = "Этот заказ не может быть возвращён, так как оплата не была произведена.";
            return RedirectToPage(new { id });
        }
        
        // --- THIS IS THE FIX ---
        // We must provide the gateway name and the reference to the payment service
        var result = await _paymentService.RefundOrderAsync(order);
        // --- END OF FIX ---
        
        if (result.StartsWith("Refund initiated")) // Check for success prefix
        {
            // 1. RETURN ITEMS TO STOCK (with variant support)
            var productRepo = _unitOfWork.Repository<Product>();
            var variantRepo = _unitOfWork.Repository<ProductVariant>();
            foreach (var item in order.OrderItems)
            {
                if (item.ItemOrdered.ProductVariantId.HasValue)
                {
                    var variant = await variantRepo.GetByIdAsync(item.ItemOrdered.ProductVariantId.Value);
                    if (variant != null)
                    {
                        variant.QuantityInStock += item.Quantity;
                        variantRepo.Update(variant);
                    }
                }
                else
                {
                    var product = await productRepo.GetByIdAsync(item.ItemOrdered.ProductId);
                    if (product != null)
                    {
                        product.QuantityInStock += item.Quantity;
                        productRepo.Update(product);
                    }
                }
            }

            // 2. UPDATE ORDER STATUS
            order.Status = OrderStatus.Refunded;

            // 3. SAVE CHANGES
            await _unitOfWork.Complete();
        }
        else
        {
            TempData["ErrorMessage"] = $"Ошибка возврата средств: {result}";
        }

        return RedirectToPage(new { id });
    }

    public string GetStatusBadgeClass(string status) => status switch
    {
        "PaymentReceived" => "text-bg-success",
        "Pending" => "text-bg-warning",
        "PaymentFailed" => "text-bg-danger",
        "Refunded" => "text-bg-secondary",
        _ => "text-bg-light"
    };

    public string GetDeliveryStatusBadgeClass(string status) => status switch
    {
        "Delivered" => "text-bg-success",
        "Processing" => "text-bg-info",
        "Shipped" => "text-bg-primary",
        "OutForDelivery" => "text-bg-dark",
        _ => "text-bg-light"
    };

    public string GetDeliveryIconClass(string status) => status switch
    {
        "Delivered" => "bi bi-check-circle-fill",
        "Processing" => "bi bi-clock",
        "Shipped" => "bi bi-truck",
        "OutForDelivery" => "bi bi-bicycle",
        _ => "bi bi-question-circle"
    };

    private string GetDefaultNoteForStatus(string status) => status switch
    {
        "Shipped" => "Your order has been shipped.",
        "OutForDelivery" => "Your order is out for delivery with the courier.",
        "Delivered" => "Your order has been successfully delivered.",
        _ => string.Empty
    };
}