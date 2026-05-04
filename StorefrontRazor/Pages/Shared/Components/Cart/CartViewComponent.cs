using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace StorefrontRazor.Pages.Shared.Components.Cart;

public class CartViewComponent : ViewComponent
{
    private readonly ICartService _cartService;
    public CartViewComponent(ICartService cartService) => _cartService = cartService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var cart = await _cartService.GetCartAsync();
        var itemCount = cart?.Items.Sum(item => item.Quantity) ?? 0;
        return View(itemCount);
    }
}