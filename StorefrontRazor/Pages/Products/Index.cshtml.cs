using System.Security.Claims;
using Core.DTOs;
using Core.Entities;
using Core.Extensions;
using Core.Interfaces;
using Core.Paging;
using Core.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Products;

public class IndexModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartService _cartService;
    private readonly IWishlistService _wishlistService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly ISiteSettingsService _siteSettings;
    private readonly IAIService _aiService;
    private readonly IAdaptiveRecommendationService _recommendationService;

    // Properties to hold the data for the view
    public Pagination<ProductDto> ProductPagination { get; set; } = null!;
    public IReadOnlyList<ProductBrand> Brands { get; set; } = null!;
    public IReadOnlyList<ProductType> Types { get; set; } = null!;
    public IReadOnlyList<Category> Categories { get; set; } = null!;
    public ShoppingCart Cart { get; set; } = null!;
    public bool IsAISearchEnabled { get; set; }
    public bool UsedAISearch { get; set; }
    public List<ProductDto> PersonalizedRecommendations { get; set; } = new();
    public List<ProductDto> PopularRecommendations { get; set; } = new();

    // Properties to capture filter values from the URL query string.
    // [BindProperty(SupportsGet = true)] is crucial for this to work on GET requests.
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int? BrandFilter { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int? TypeFilter { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int? CategoryFilter { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int? RatingFilter { get; set; } = null!;

    public string? PromoHtmlContent { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool AISearch { get; set; } = false;

    [BindProperty(SupportsGet = true)]
    public string? SortOrder { get; set; } = "name";

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public ProductSpecParams ProductParams { get; set; } = new();

    public IndexModel(IUnitOfWork unitOfWork, ISiteSettingsService siteSettings, ICartService cartService, IWishlistService wishlistService, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IAIService aiService, IAdaptiveRecommendationService recommendationService)
    {
        _unitOfWork = unitOfWork;
        _siteSettings = siteSettings;
        _cartService = cartService;
        _wishlistService = wishlistService;
        _signInManager = signInManager;
        _userManager = userManager;
        _aiService = aiService;
        _recommendationService = recommendationService;
    }

    public async Task OnGetAsync()
    {
        // Check if AI is enabled
        IsAISearchEnabled = _aiService.IsEnabled;
        
        // If AI search is requested and there's a search term, use semantic search
        if (AISearch && IsAISearchEnabled && !string.IsNullOrWhiteSpace(SearchTerm))
        {
            await PerformAISearchAsync();
            return;
        }
        
        // Standard search/filter logic
        // 1. Populate the specification parameters from the URL
        // We now correctly initialize the ProductSpecParams object.
        var specParams = new ProductSpecParams
        {
            PageIndex = this.PageIndex,
            PageSize = 12,
            Search = this.SearchTerm,
            Sort = this.SortOrder,
            BrandId = this.BrandFilter,
            TypeId = this.TypeFilter,
            CategoryId = this.CategoryFilter,
            MinRating = this.RatingFilter
        };

        // 2. Fetch the paginated list of products
        var spec = new ProductSpecification(specParams);
        var products = await _unitOfWork.Repository<Product>().ListAsync(spec);
        var totalItems = await _unitOfWork.Repository<Product>().CountAsync(new ProductWithFiltersForCountSpecification(specParams));

        var productDtos = products.Select(p => p.ToDto()).ToList();
        ProductPagination = new Pagination<ProductDto>(PageIndex, specParams.PageSize, totalItems, productDtos);

        // 3. Fetch the lists for filter controls
        Brands = await _unitOfWork.Repository<ProductBrand>().ListAllAsync();
        Types = await _unitOfWork.Repository<ProductType>().ListAllAsync();
        Categories = await _unitOfWork.Repository<Category>().ListAllAsync();

        // 4. Optionally, you can also fetch the current cart item count for display
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
        
        // --- 4. ADD THIS SEO BLOCK ---
        var settings = await _siteSettings.GetSettingsAsync();
        var storeName = settings.GetValueOrDefault("StoreName", "Devs Store");
        var logoUrl = settings.GetValueOrDefault("StoreLogoUrl", ""); // Use logo as default image

        var title = "All Products";
        var description = $"Browse and shop our full collection of products at {storeName}. Find the best deals on top brands, explore categories, read reviews, and enjoy fast shipping on every order.";

        // Dynamically change title/description based on filters
        if (ProductParams.CategoryId.HasValue)
        {
            var category = Categories.FirstOrDefault(c => c.Id == ProductParams.CategoryId.Value);
            if (category != null)
            {
                title = $"{category.Name} Products";
                description = $"Shop our selection of {category.Name} products at {storeName}. Browse top-rated items, compare prices, read customer reviews, and find the perfect product for your needs.";
            }
        }
        else if (ProductParams.BrandId.HasValue)
        {
            var brand = Brands.FirstOrDefault(b => b.Id == ProductParams.BrandId.Value);
            if (brand != null)
            {
                title = $"{brand.Name} Products";
                description = $"Explore the full range of {brand.Name} products at {storeName}. Discover quality items, read customer reviews, compare options, and shop with confidence.";
            }
        }
        else if (!string.IsNullOrEmpty(ProductParams.Search))
        {
            title = $"Search results for '{ProductParams.Search}'";
            description = $"Showing search results for '{ProductParams.Search}' at {storeName}. Browse matching products, compare prices, and find exactly what you're looking for.";
        }

        ViewData["Title"] = $"{title} - {storeName}";
        ViewData["Description"] = description;
        ViewData["ImageUrl"] = logoUrl; // Use the store's main logo for list pages
        // --- END OF SEO BLOCK ---

        // --- Load personalized recommendations for logged-in users ---
        if (_signInManager.IsSignedIn(User))
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var adaptiveProducts = await _recommendationService.GetAdaptiveRecommendationsAsync(user.Id, 6);
                    PersonalizedRecommendations = adaptiveProducts.Select(p => p.ToDto()).ToList();

                    var popularProducts = await _recommendationService.GetPopularProductsAsync(6);
                    PopularRecommendations = popularProducts.Select(p => p.ToDto()).ToList();
                }
            }
            catch { /* Recommendations are non-critical */ }
        }
    }

    // --- ADD THIS NEW HANDLER ---
    public async Task<IActionResult> OnGetProductListPartialAsync()
    {
        // This handler re-uses the exact same logic as your main OnGetAsync method
        // to ensure filtering, sorting, and searching are consistent.
        var specParams = new ProductSpecParams
        {
            PageIndex = this.PageIndex,
            PageSize = 12,
            Search = this.SearchTerm,
            Sort = this.SortOrder,
            BrandId = this.BrandFilter,
            TypeId = this.TypeFilter,
            CategoryId = this.CategoryFilter,
            MinRating = this.RatingFilter
        };

        var spec = new ProductSpecification(specParams);
        var products = await _unitOfWork.Repository<Product>().ListAsync(spec);
        var totalItems = await _unitOfWork.Repository<Product>().CountAsync(new ProductWithFiltersForCountSpecification(specParams));

        var productDtos = products.Select(p => p.ToDto()).ToList();
        ProductPagination = new Pagination<ProductDto>(PageIndex, specParams.PageSize, totalItems, productDtos);

        Cart = await _cartService.GetCartAsync();

        // Instead of returning the whole page, return just the partial view
        return Partial("_ProductListPartial", this);
    }

    public async Task<IActionResult> OnPostAddToCartJsonAsync(int productId)
    {
        var cart = await _cartService.AddItemToCartAsync(productId, null, 1);
        return new JsonResult(new { itemCount = cart.Items.Sum(i => i.Quantity) });
    }

    public async Task<IActionResult> OnPostUpdateCartJsonAsync(int productId, int quantity)
    {
        var cart = await _cartService.SetItemQuantityAsync(productId, null, quantity);
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);

        return new JsonResult(new
        {
            itemCount = cart.Items.Sum(i => i.Quantity),
            newQuantity = cart.Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0,
            stock = product.QuantityInStock // This is the crucial part that was missing
        });
    }


    public Dictionary<string, string> GetAllRouteData(string remove = null, Dictionary<string, string> add = null)
    {
        var routeData = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(SearchTerm)) routeData["SearchTerm"] = SearchTerm;
        if (BrandFilter.HasValue) routeData["BrandFilter"] = BrandFilter.Value.ToString();
        if (TypeFilter.HasValue) routeData["TypeFilter"] = TypeFilter.Value.ToString();
        if (CategoryFilter.HasValue) routeData["CategoryFilter"] = CategoryFilter.Value.ToString();
        if (RatingFilter.HasValue) routeData["RatingFilter"] = RatingFilter.Value.ToString();
        if (!string.IsNullOrEmpty(SortOrder)) routeData["SortOrder"] = SortOrder;

        if (!string.IsNullOrEmpty(remove) && routeData.ContainsKey(remove))
        {
            routeData.Remove(remove);
        }

        if (add != null)
        {
            foreach (var item in add)
            {
                routeData[item.Key] = item.Value;
            }
        }

        return routeData;
    }

    public async Task<IActionResult> OnPostToggleWishlistAsync(int productId)
    {
        if (!_signInManager.IsSignedIn(User)) return Unauthorized();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isInWishlist = await _wishlistService.IsItemInWishlistAsync(userId, productId);

        if (isInWishlist)
        {
            var wishlist = await _wishlistService.GetOrCreateWishlistForUserAsync(userId);
            var item = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null) await _wishlistService.RemoveItemFromWishlistAsync(item.Id);
        }
        else
        {
            await _wishlistService.AddItemToWishlistAsync(userId, productId);
        }
        return new JsonResult(new { isInWishlist = !isInWishlist });
    }

    private async Task PerformAISearchAsync()
    {
        UsedAISearch = true;
        
        try
        {
            // Use AI semantic search
            var searchResults = await _aiService.SemanticSearchAsync(SearchTerm!, 24);
            
            var productDtos = searchResults.Select(r => r.Product.ToDto()).ToList();
            
            // Create a simple pagination object (no actual paging for AI search results)
            ProductPagination = new Pagination<ProductDto>(1, productDtos.Count > 0 ? productDtos.Count : 12, productDtos.Count, productDtos);
        }
        catch
        {
            // Fall back to empty results if AI search fails
            ProductPagination = new Pagination<ProductDto>(1, 12, 0, []);
        }

        // Still need to load filter options for the sidebar
        Brands = await _unitOfWork.Repository<ProductBrand>().ListAllAsync();
        Types = await _unitOfWork.Repository<ProductType>().ListAllAsync();
        Categories = await _unitOfWork.Repository<Category>().ListAllAsync();
        Cart = await _cartService.GetCartAsync();

        var promoSpec = new BaseSpecification<ContentBlock>(cb => cb.Key == "home-page-promo-html");
        var promoBlock = await _unitOfWork.Repository<ContentBlock>().GetEntityWithSpec(promoSpec);
        PromoHtmlContent = promoBlock?.IsHtml == true ? promoBlock.Content : null;

        // Set SEO metadata for AI search
        var settings = await _siteSettings.GetSettingsAsync();
        var storeName = settings.GetValueOrDefault("StoreName", "Devs Store");
        ViewData["Title"] = $"AI Search: {SearchTerm} | {storeName}";
        ViewData["MetaDescription"] = $"AI-powered search results for '{SearchTerm}' at {storeName}.";
    }
}
