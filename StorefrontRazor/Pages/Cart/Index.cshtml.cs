using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StorefrontRazor.Services;
using System.Security.Claims;

using Core.Specifications;

namespace StorefrontRazor.Pages.Cart;

public class IndexModel : PageModel
{
    private readonly ICartService _cartService;
    public CheckoutService Checkout { get; }
    private readonly IUnitOfWork _unitOfWork;

    public string? PromoHtmlContent { get; set; }


    public IndexModel(ICartService cartService, CheckoutService checkout, IUnitOfWork unitOfWork)
    {
        _cartService = cartService;
        Checkout = checkout;
        _unitOfWork = unitOfWork;
    }



    public ShoppingCart Cart { get; set; } = null!;

    // This property will bind to the text input for the voucher code form
    [BindProperty]
    public string VoucherCode { get; set; } = null!;

    // This property will hold any error messages for the coupon form
    [TempData]
    public string CouponErrorMessage { get; set; } = null!;

    public int Step { get; } = 0;

    public async Task OnGetAsync()
    {
        Cart = await _cartService.GetCartAsync();

        var promoSpec = new BaseSpecification<ContentBlock>(cb => cb.Key == "home-page-promo-html");
        var promoBlock = await _unitOfWork.Repository<ContentBlock>().GetEntityWithSpec(promoSpec);

        if (promoBlock != null && promoBlock.IsHtml)
        {
            PromoHtmlContent = promoBlock.Content;
        }
        else
        {
            PromoHtmlContent = null;
        }
    }

    // --- Handler for applying a coupon ---
    public async Task<IActionResult> OnPostApplyCouponAsync()
    {
        if (string.IsNullOrWhiteSpace(VoucherCode))
        {
            CouponErrorMessage = "Пожалуйста, введите код купона.";
            return RedirectToPage();
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail))
        {
            CouponErrorMessage = "Для использования купонов необходимо войти в аккаунт.";
            return RedirectToPage();
        }

        var success = await _cartService.ApplyCouponAsync(VoucherCode, userEmail);
        if (!success)
        {
            // This message now covers all failure reasons, including our new check.
            CouponErrorMessage = "Этот код купона недействителен или не применим к товарам в вашей корзине.";
        }
        return RedirectToPage();
    }

    // --- Handler for removing a coupon ---
    public async Task<IActionResult> OnPostRemoveCouponAsync()
    {
        await _cartService.RemoveCouponAsync();
        return RedirectToPage();
    }

    // --- Existing handlers for non-JS fallback ---
    public async Task<IActionResult> OnPostRemoveItemAsync(int productId)
    {
        var cart = await _cartService.GetCartAsync();
        var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);
        if (itemToRemove != null)
        {
            cart.Items.Remove(itemToRemove);
            await _cartService.UpdateCartAsync(cart);
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateQuantityAsync(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            return await OnPostRemoveItemAsync(productId);
        }

        var cart = await _cartService.GetCartAsync();
        var itemToUpdate = cart.Items.FirstOrDefault(item => item.ProductId == productId);
        if (itemToUpdate != null)
        {
            itemToUpdate.Quantity = quantity;
            await _cartService.UpdateCartAsync(cart);
        }
        return RedirectToPage();
    }

    // === ADD NEW JSON HANDLER FOR REMOVING ITEMS ===
    /* public async Task<IActionResult> OnPostRemoveItemJsonAsync(int productId)
    {
        var cart = await _cartService.GetCartAsync();
        var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);
        if (itemToRemove != null)
        {
            cart.Items.Remove(itemToRemove);
            await _cartService.UpdateCartAsync(cart);
        }
        return new JsonResult(CreateCartViewModel(cart));
    } */

    public async Task<IActionResult> OnPostRemoveItemJsonAsync(int productId, int? productVariantId)
    {
        var cart = await _cartService.GetCartAsync();
        // Use both IDs to find the exact item to remove
        var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId && item.ProductVariantId == productVariantId);
        if (itemToRemove != null)
        {
            cart.Items.Remove(itemToRemove);
            await _cartService.UpdateCartAsync(cart);
        }
        return new JsonResult(CreateCartViewModel(cart));
    }

    // === ADD NEW JSON HANDLER FOR UPDATING QUANTITY ===
    /* public async Task<IActionResult> OnPostUpdateQuantityJsonAsync(int productId, int quantity)
    {
        var cart = await _cartService.GetCartAsync();
        if (quantity <= 0)
        {
            var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null) cart.Items.Remove(itemToRemove);
        }
        else
        {
            var itemToUpdate = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (itemToUpdate != null)
            {
                itemToUpdate.Quantity = quantity;
            }
        }
        await _cartService.UpdateCartAsync(cart);
        return new JsonResult(CreateCartViewModel(cart));
    } */

    public async Task<IActionResult> OnPostUpdateQuantityJsonAsync(int productId, int? productVariantId, int quantity)
    {
        // Use the existing SetItemQuantityAsync which is already correct
        var cart = await _cartService.SetItemQuantityAsync(productId, productVariantId, quantity);
        return new JsonResult(CreateCartViewModel(cart));
    }


    private object CreateCartViewModel(ShoppingCart cart)
    {
        var subtotal = cart.Items.Sum(i => i.Price * i.Quantity);
        var itemCount = cart.Items.Sum(i => i.Quantity);

        var discount = 0m;
        if (cart.Coupon != null)
        {
            if (cart.Coupon.AmountOff.HasValue)
            {
                discount = cart.Coupon.AmountOff.Value;
            }
            else if (cart.Coupon.PercentOff.HasValue)
            {
                discount = subtotal * (cart.Coupon.PercentOff.Value / 100);
            }
        }

        // On the cart page, delivery is always 0. It gets calculated in checkout.
        var delivery = 0m;
        var total = subtotal - discount + delivery;

        // === THIS IS THE FIX: Added all summary fields to the response ===
        return new
        {
            itemCount,
            subtotal = subtotal.ToString("C"),
            delivery = delivery.ToString("C"),
            discount = discount.ToString("C"),
            discountCode = cart.Coupon?.PromotionCode,
            total = total.ToString("C"),
            items = cart.Items.Select(i => new
            {
                productId = i.ProductId,
                productVariantId = i.ProductVariantId,
                quantity = i.Quantity,
                itemTotal = (i.Price * i.Quantity).ToString("C")
            })
        };
    }
}
