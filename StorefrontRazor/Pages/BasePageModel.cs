using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages;

public class BasePageModel : PageModel
{
    // This is the original handler for non-javascript users. No changes needed.
    public async Task<IActionResult> OnPostAddToCartAsync([FromServices] ICartService cartService, int productId, int quantity = 1)
    {
        await cartService.AddItemToCartAsync(productId,null, quantity);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateCartAsync([FromServices] ICartService cartService, int productId, int quantity)
    {
        await cartService.SetItemQuantityAsync(productId,null, quantity);
        return RedirectToPage();
    }

    

    // AJAX handler for adding a new item
    /* public async Task<IActionResult> OnPostAddToCartJsonAsync([FromServices] ICartService cartService, int productId, int quantity = 1)
    {
        await cartService.AddItemToCartAsync(productId, quantity);
        var updatedCart = await cartService.GetCartAsync();
        var newCartItemCount = updatedCart?.Items.Sum(item => item.Quantity) ?? 0;
        return new JsonResult(new { itemCount = newCartItemCount });
    }

    // === ADD THIS NEW JSON HANDLER FOR UPDATING QUANTITY ===
    public async Task<IActionResult> OnPostUpdateCartJsonAsync([FromServices] ICartService cartService, int productId, int quantity)
    {
        await cartService.SetItemQuantityAsync(productId, quantity);
        var updatedCart = await cartService.GetCartAsync();
        var itemCount = updatedCart?.Items.Sum(item => item.Quantity) ?? 0;

        return new JsonResult(new
        {
            itemCount, // The new total count for the nav badge
            newQuantity = updatedCart.Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0
        });
    } */
}