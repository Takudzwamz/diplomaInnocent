using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text.Json; // <-- ADD THIS

namespace StorefrontRazor.Pages.Checkout;

public class PaymentPostModel : PageModel
{
    // --- REMOVE [BindProperty] ---
    public Dictionary<string, string> PostData { get; set; } = new();
    public string GatewayUrl { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        // --- THIS IS THE FIX ---
        var postDataJson = TempData["PayFast_PostData"] as string;
        var gatewayUrl = TempData["PayFast_GatewayUrl"] as string;

        if (string.IsNullOrEmpty(postDataJson) || string.IsNullOrEmpty(gatewayUrl))
        {
            TempData["ErrorMessage"] = "Сессия оплаты истекла. Попробуйте ещё раз.";
            return RedirectToPage("/Cart/Index");
        }

        PostData = JsonSerializer.Deserialize<Dictionary<string, string>>(postDataJson) ?? new Dictionary<string, string>();
        GatewayUrl = gatewayUrl;
        
        return Page(); // Proceed to render the page
        // --- END OF FIX ---
    }
}