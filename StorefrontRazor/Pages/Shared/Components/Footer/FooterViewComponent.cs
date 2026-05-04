using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace StorefrontRazor.Pages.Shared.Components.Footer;

public class FooterViewComponent : ViewComponent
{
    // 1. Inject IUnitOfWork instead of ISiteSettingsService
    private readonly IUnitOfWork _unitOfWork;

    public FooterViewComponent(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 2. Define the exact keys we need for the footer
        var footerKeys = new[] {
            "footer-about-us",
            "social-facebook-url",
            "social-twitter-url",
            "social-instagram-url",
            "social-linkedin-url",
            "social-whatsapp-url"
        };

        // 3. Create a specification to fetch only these keys
        var spec = new BaseSpecification<ContentBlock>(cb => footerKeys.Contains(cb.Key));
        var contentBlocks = await _unitOfWork.Repository<ContentBlock>().ListAsync(spec);

        // 4. Convert the list into the Dictionary<string, string> that the view expects
        var settings = contentBlocks.ToDictionary(cb => cb.Key, cb => cb.Content);

        // 5. Pass the fresh data (not cached) to the view
        return View(settings);
    }
}