using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Hero;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StoreContext _context;
    public IndexModel(IUnitOfWork unitOfWork, StoreContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public IReadOnlyList<HeroSlide> Slides { get; set; } = new List<HeroSlide>();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Manage Hero Carousel";
        Slides = await _unitOfWork.Repository<HeroSlide>().ListAllAsync();
    }

    // You can add a Delete handler here if needed, following the pattern from other admin pages.
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var slide = await _context.HeroSlides.FindAsync(id);
        if (slide == null)
        {
            return NotFound();
        }

        _context.HeroSlides.Remove(slide);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}