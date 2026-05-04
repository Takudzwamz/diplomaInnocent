using Core.DTOs;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace StorefrontRazor.Pages.Orders;

[Authorize]
public class FailedModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public FailedModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty(SupportsGet = true)]
    public string Reference { get; set; } = string.Empty;

    public OrderDto Order { get; set; } = null!; // Initialize to null

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["Title"] = "Оплата отменена";

        if (string.IsNullOrEmpty(Reference))
        {
            return RedirectToPage("/Index");
        }

        var email = User.FindFirstValue(ClaimTypes.Email);
        
        // --- THIS IS THE FIX ---
        // We must pass 'true' as the second argument to match the
        // constructor for finding by PaymentReference.
        var spec = new OrderSpecification(Reference, true); 
        // --- END OF FIX ---
        
        var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

        // Ensure the order exists and belongs to the current user
        if (order == null || order.BuyerEmail != email)
        {
            return RedirectToPage("./Index");
        }

        Order = order.ToDto();
        return Page();
    }
}