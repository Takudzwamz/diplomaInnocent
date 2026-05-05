using Core.DTOs;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace StorefrontRazor.Pages.Orders;

[Authorize]
public class ConfirmationModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdaptiveRecommendationService _adaptiveRecommendationService;
    private readonly UserManager<AppUser> _userManager;

    public ConfirmationModel(IUnitOfWork unitOfWork, IAdaptiveRecommendationService adaptiveRecommendationService, UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _adaptiveRecommendationService = adaptiveRecommendationService;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string Reference { get; set; } = string.Empty;

    public OrderDto Order { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public List<ProductDto> CollaborativeRecommendations { get; set; } = new();
    public List<ProductDto> PopularRecommendations { get; set; } = new();
    public List<ProductDto> AdaptiveRecommendations { get; set; } = new();

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
                    await LoadRecommendationsAsync();
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

    private async Task LoadRecommendationsAsync()
    {
        try
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = !string.IsNullOrEmpty(email) ? await _userManager.FindByEmailAsync(email) : null;
            if (user == null) return;

            try
            {
                var collab = await _adaptiveRecommendationService
                    .GetCollaborativeRecommendationsAsync(user.Id, count: 4);
                CollaborativeRecommendations = collab.Select(p => p.ToDto()).ToList();
            }
            catch { }

            try
            {
                var adaptive = await _adaptiveRecommendationService
                    .GetAdaptiveRecommendationsAsync(user.Id, count: 4);
                AdaptiveRecommendations = adaptive.Select(p => p.ToDto()).ToList();
            }
            catch { }

            try
            {
                var popular = await _adaptiveRecommendationService
                    .GetPopularProductsAsync(count: 4);
                PopularRecommendations = popular.Select(p => p.ToDto()).ToList();
            }
            catch { }
        }
        catch { }
    }
}
