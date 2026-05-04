using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages;

public class ContactModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public ContactModel(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    public string PhoneNumber { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string PhysicalAddress { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Свяжитесь с нами";
        ViewData["Description"] = "Свяжитесь с нашей командой. Наш телефон, email и адрес. Мы будем рады вашему обращению.";
        
        var contactKeys = new[] { "contact-phone", "contact-email", "contact-address" };
        var spec = new BaseSpecification<ContentBlock>(cb => contactKeys.Contains(cb.Key));
        var contentBlocks = await _unitOfWork.Repository<ContentBlock>().ListAsync(spec);

        PhoneNumber = contentBlocks.FirstOrDefault(c => c.Key == "contact-phone")?.Content ?? "Не указано";
        EmailAddress = contentBlocks.FirstOrDefault(c => c.Key == "contact-email")?.Content ?? "Не указано";
        PhysicalAddress = contentBlocks.FirstOrDefault(c => c.Key == "contact-address")?.Content ?? "Не указано";
    }
}