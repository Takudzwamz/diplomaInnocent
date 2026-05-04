using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace StorefrontRazor.Pages.Admin.Content;

public class EditPolicyModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public EditPolicyModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public PolicyInputModel PolicyInput { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "Edit Return Policy";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var spec = new BaseSpecification<ContentBlock>(cb => cb.Key == "return-policy");
        var policyBlock = await _unitOfWork.Repository<ContentBlock>().GetEntityWithSpec(spec);

        if (policyBlock == null)
        {
            return NotFound("The 'return-policy' content block was not found.");
        }

        // --- Build the final HTML from the simple inputs ---
        var nonReturnableList = new StringBuilder();
        var items = PolicyInput.NonReturnableItems.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in items)
        {
            nonReturnableList.Append($"<li>{item}</li>");
        }

        policyBlock.Content = $@"
            <h1 class='mb-4'>Our Return Policy</h1>
            <p>We want you to shop with confidence. If for any reason you are not completely satisfied with your purchase, we’re here to help.</p>
            <h2 class='mt-4'>Returns</h2>
            <p>You have <strong>{PolicyInput.ReturnWindowDays} calendar days</strong> from the date you received your item to request a return. To be eligible, your item must be unused, in the same condition that you received it, and in the original packaging.</p>
            <h2 class='mt-4'>Refunds</h2>
            <p>Once we receive your return, we will inspect it and notify you of the status. If approved, your refund will be processed to your original method of payment within <strong>{PolicyInput.RefundTimeframe}</strong>. Shipping costs are non-refundable.</p>
            <h2 class='mt-4'>Non-Returnable Items</h2>
            <ul>{nonReturnableList}</ul>
            <h2 class='mt-4'>How to Start a Return</h2>
            <p>To initiate a return, please email us at <a href='mailto:{PolicyInput.ContactEmail}'>{PolicyInput.ContactEmail}</a> with your order number and details about the product. Our team will guide you through the process.</p>
            <p class='mt-4'><em>Note: This return policy does not affect your statutory rights.</em></p>";

        // Ensure the IsHtml flag is set to true
        policyBlock.IsHtml = true;

        _unitOfWork.Repository<ContentBlock>().Update(policyBlock);
        await _unitOfWork.Complete();

        return RedirectToPage("./Index");
    }
}