using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Account;

[Authorize]
public class MyWishlistModel : PageModel
{
    private readonly IWishlistService _wishlistService;

    public MyWishlistModel(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    public List<WishlistItem> WishlistItems { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var wishlist = await _wishlistService.GetOrCreateWishlistForUserAsync(userId);
        WishlistItems = wishlist.Items;
    }

    public async Task<IActionResult> OnPostRemoveItemAsync(int itemId)
    {
        await _wishlistService.RemoveItemFromWishlistAsync(itemId);
        return RedirectToPage();
    }
}