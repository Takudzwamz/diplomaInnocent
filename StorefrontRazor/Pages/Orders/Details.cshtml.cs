using System.Security.Claims;
using Core.DTOs;
using Core.Entities; 
using Microsoft.AspNetCore.Identity;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace StorefrontRazor.Pages.Orders;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly UserManager<AppUser> _userManager;

    public DetailsModel(IUnitOfWork unitOfWork, IPaymentService paymentService, UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _userManager = userManager;
    }
    
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public OrderDto Order { get; set; }

    public HashSet<int> ReviewedProductIds { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var spec = new OrderSpecification(email, Id);
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var reviewSpec = new BaseSpecification<ProductReview>(r => r.AppUserId == user.Id);
            var userReviews = await _unitOfWork.Repository<ProductReview>().ListAsync(reviewSpec);
            ReviewedProductIds = userReviews.Select(r => r.ProductId).ToHashSet();
        }

        Order = order.ToDto();
        return Page();
    }

    /* public async Task<IActionResult> OnPostRetryPaymentAsync()
    {
        var (order, authUrl) = await _paymentService.CreateRetryPaymentTransactionAsync(Id);
        if (!string.IsNullOrEmpty(authUrl))
        {
            return Redirect(authUrl);
        }
        
        TempData["ErrorMessage"] = "This order cannot be paid for. An item may be out of stock.";
        return RedirectToPage(new { id = Id });
    } */

    public async Task<IActionResult> OnPostRetryPaymentAsync()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var spec = new OrderSpecification(email, Id);
        var orderToRetry = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        if (orderToRetry == null)
        {
            return NotFound();
        }
        
        try
        {
            // Call the service with the gateway name AND the order ID
            var (order, authUrl) = await _paymentService.CreateRetryPaymentTransactionAsync(orderToRetry.PaymentGatewayName, Id);
            
            if (!string.IsNullOrEmpty(authUrl))
            {
                // Paystack: Redirect the user
                return Redirect(authUrl);
            }
            
            if (order != null) // This means the gateway (PayFast) is returning postData
            {
                // We must re-call the CreatePaymentTransactionAsync to get the postData
                var (postAuthUrl, postData) = await _paymentService.CreatePaymentTransactionAsync(order);
                if (postData != null)
                {
                    // PayFast
                    var settings = await _unitOfWork.Repository<SiteSetting>().ListAllAsync();
                    var siteMode = settings.FirstOrDefault(s => s.Key == "Payment_SiteMode")?.Value ?? "Test";
                    var useSandbox = (siteMode == "Test");
                    var gatewayUrl = useSandbox ? "https://sandbox.payfast.co.za/eng/process" : "https://www.payfast.co.za/eng/process";
                    
                    TempData["PayFast_PostData"] = JsonSerializer.Serialize(postData);
                    TempData["PayFast_GatewayUrl"] = gatewayUrl;
                    return RedirectToPage("/Checkout/PaymentPost");
                }
            }
        }
        catch (Exception ex)
        {
            // This will catch the "stock" exception
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage(new { id = Id });
        }
        
        TempData["ErrorMessage"] = "Этот заказ не может быть оплачен. Возможно, товар закончился.";
        return RedirectToPage(new { id = Id });
    }

    // --- THIS IS THE FIX ---
    // Helper methods for status badges are now fully implemented.
    public string GetStatusBadgeClass(string status) => status switch
    {
        "PaymentReceived" => "text-bg-success",
        "Pending" => "text-bg-warning",
        "PaymentFailed" => "text-bg-danger",
        "Refunded" => "text-bg-secondary",
        "PaymentMismatch" => "text-bg-info",
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
    // --- END OF FIX ---

    public string GetDeliveryIconClass(string status) => status switch
    {
        "Processing" => "bi bi-gear-fill",
        "Shipped" => "bi bi-box-arrow-up-right",
        "OutForDelivery" => "bi bi-truck",
        "Delivered" => "bi bi-check-circle-fill",
        _ => "bi bi-question-circle"
    };
}