using Core.DTOs;
using Core.Entities;
using Core.Extensions;
using Core.Interfaces;
using Core.Paging;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Products;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;

    public IndexModel(IUnitOfWork unitOfWork, IImageService imageService)
    {
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    public Pagination<ProductDto> ProductPagination { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public bool CanCreateProducts { get; private set; }
    public List<string> MissingPrerequisites { get; private set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Управление товарами";
        // --- START: ADD THIS PREREQUISITE CHECK ---
        var brandCount = await _unitOfWork.Repository<ProductBrand>().CountAsync(null!);
        var typeCount = await _unitOfWork.Repository<ProductType>().CountAsync(null!);
        var categoryCount = await _unitOfWork.Repository<Category>().CountAsync(null!);
        var deliveryMethodCount = await _unitOfWork.Repository<DeliveryMethod>().CountAsync(null!);

        if (brandCount == 0) MissingPrerequisites.Add("Brands");
        if (typeCount == 0) MissingPrerequisites.Add("Types");
        if (categoryCount == 0) MissingPrerequisites.Add("Categories");
        if (deliveryMethodCount == 0) MissingPrerequisites.Add("DeliveryMethods");

        CanCreateProducts = !MissingPrerequisites.Any();

        var productParams = new ProductSpecParams { PageIndex = PageIndex, PageSize = 10 };

        var spec = new ProductSpecification(productParams);
        var countSpec = new ProductWithFiltersForCountSpecification(productParams);

        var totalItems = await _unitOfWork.Repository<Product>().CountAsync(countSpec);
        var products = await _unitOfWork.Repository<Product>().ListAsync(spec);

        var data = products.Select(p => p.ToDto()).ToList();
        ProductPagination = new Pagination<ProductDto>(PageIndex, productParams.PageSize, totalItems, data);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var spec = new ProductSpecification(id, withImages: true);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);

        if (product == null) return NotFound();

        // Delete all associated images from Cloudinary
        foreach (var image in product.Images)
        {
            await _imageService.DeleteImageAsync(image.Url);
        }

        // 2. Manually remove the dependent variants first.
        // Because the Product -> Variant relationship is configured with OnDelete.Restrict,
        // the change tracker won't do this automatically.
        var variantRepo = _unitOfWork.Repository<ProductVariant>();
        foreach (var variant in product.Variants.ToList()) // Use ToList() to create a copy and avoid modification errors
        {
            variantRepo.Remove(variant);
        }

        _unitOfWork.Repository<Product>().Remove(product);
        await _unitOfWork.Complete();

        return RedirectToPage("./Index", new { PageIndex = this.PageIndex });
    }
}
