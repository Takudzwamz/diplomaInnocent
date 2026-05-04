using Core.DTOs;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Orders;

[Authorize]
public class ConfirmationModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmationModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty(SupportsGet = true)]
    public string Reference { get; set; }

    public OrderDto Order { get; set; }
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(Reference))
        {
            return RedirectToPage("/Index");
        }

        // We poll the database a few times to give the webhook a chance to arrive.
        const int maxRetries = 5;
        for (int i = 0; i < maxRetries; i++)
        {
            var spec = new OrderSpecification(Reference, true);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

            if (order != null)
            {
                // Check if the webhook has updated the status yet
                if (order.Status == OrderStatus.PaymentReceived)
                {
                    Order = order.ToDto();
                    IsSuccess = true;
                    return Page();
                }
                
                // If it's failed, show the failure message immediately
                if (order.Status == OrderStatus.PaymentFailed)
                {
                    Order = order.ToDto();
                    IsSuccess = false;
                    return Page();
                }
            }
            
            // Wait for 2 seconds before checking again
            await Task.Delay(2000);
        }
        
        // If after several retries the order is still pending, we assume it's still processing.
        IsSuccess = false; // We can set this to false to show a "processing" message
        return Page();
    }
}
