using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
// using Infrastructure.Services.PaymentHelpers; // No longer needed

namespace StorefrontRazor.Pages.Webhooks;

[IgnoreAntiforgeryToken]
public class PayFastModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PayFastModel> _logger;

    public PayFastModel(IPaymentService paymentService, ILogger<PayFastModel> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            _logger.LogInformation("PayFast ITN webhook starting..."); // Added a log
            
            // 1. Read the raw form data from the body
            string payload;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                payload = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogWarning("PayFast ITN received with an empty payload.");
                return BadRequest();
            }

            // 2. Pass the raw payload to the service.
            //    We pass 'null' for the signature because our PayFast service
            //    handles all validation internally from the raw payload.
            await _paymentService.HandleWebhookAsync("PayFast", payload, null!);
            
            _logger.LogInformation("PayFast ITN webhook processed successfully.");
            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayFast ITN webhook.");
            return new StatusCodeResult(500);
        }
    }
}