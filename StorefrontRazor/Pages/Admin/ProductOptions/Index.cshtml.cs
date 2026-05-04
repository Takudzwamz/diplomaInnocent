using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.ProductOptions;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public List<ProductOptionForListDto> Options { get; set; } = [];

    public async Task OnGetAsync()
    {
        var spec = new BaseSpecification<ProductOption>();
        spec.AddInclude(o => o.Values); // Include the values to get the count
        var options = await _unitOfWork.Repository<ProductOption>().ListAsync(spec);

        Options = options.Select(o => new ProductOptionForListDto
        {
            Id = o.Id,
            Name = o.Name,
            ValueCount = o.Values.Count
        }).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var option = await _unitOfWork.Repository<ProductOption>().GetByIdAsync(id);
        if (option == null)
        {
            return NotFound();
        }

        _unitOfWork.Repository<ProductOption>().Remove(option);
        await _unitOfWork.Complete();

        return RedirectToPage();
    }
}