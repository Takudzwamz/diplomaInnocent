using System.Security.Claims;
using Core.DTOs;
using Core.Entities;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Entities.OrderAggregate; // Required for OrderStatus
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Products;

public class DetailsModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICartService _cartService;
    private readonly IWishlistService _wishlistService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ISiteSettingsService _siteSettings;
    private readonly IAIRecommendationService _aiRecommendationService;
    private readonly IAIService _aiService;
    private readonly IUserInteractionService _interactionService;
    private readonly IAdaptiveRecommendationService _adaptiveRecommendationService;
    public HashSet<int> DefaultSelectedValueIds { get; set; } = new();
    public ProductVariant? DefaultVariant { get; set; }
    public CartItem? DefaultVariantCartItem { get; set; }
    public string? PromoHtmlContent { get; set; }
    

    public DetailsModel(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, ISiteSettingsService siteSettings, ICartService cartService, IWishlistService wishlistService, SignInManager<AppUser> signInManager, IAIRecommendationService aiRecommendationService, IAIService aiService, IUserInteractionService interactionService, IAdaptiveRecommendationService adaptiveRecommendationService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _siteSettings = siteSettings;
        _cartService = cartService;
        _wishlistService = wishlistService;
        _signInManager = signInManager;
        _aiRecommendationService = aiRecommendationService;
        _aiService = aiService;
        _interactionService = interactionService;
        _adaptiveRecommendationService = adaptiveRecommendationService;
    }

    public ProductDto Product { get; set; } = null!;
    public ShoppingCart Cart { get; set; } = null!;

    public List<ProductDto> RelatedByBrand { get; set; } = [];
    public List<ProductDto> RelatedByType { get; set; } = [];
    public List<ProductDto> AIRecommendations { get; set; } = [];
    public List<ProductDto> CollaborativeRecommendations { get; set; } = [];
    public List<ProductDto> ContentBasedRecommendations { get; set; } = [];
    public List<ProductDto> PopularRecommendations { get; set; } = [];
    public List<ProductDto> AdaptiveRecommendations { get; set; } = [];
    public bool IsAIEnabled { get; set; }
    public ReviewSummary? AIReviewSummary { get; set; }

    [BindProperty]
    public CreateReviewDto NewReview { get; set; } = null!;

    public bool HasPurchasedProduct { get; set; }
    public bool HasReviewedProduct { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Cart = await _cartService.GetCartAsync();
        
        // OPTIMIZATION: Use split query to avoid cartesian explosion with variants
        // For products with many variants, this significantly improves performance by loading data in separate queries
        var query = _unitOfWork.Repository<Product>().GetQueryable()
            .Where(p => p.Id == id)
            .Include(p => p.Images)
            .Include(p => p.Reviews).ThenInclude(r => r.AppUser)
            .Include(p => p.ProductType)
            .Include(p => p.ProductBrand)
            .Include(p => p.Category)
            .Include(p => p.Coupons).ThenInclude(pc => pc.Coupon)
            .Include(p => p.Options).ThenInclude(o => o.Values)
            .Include(p => p.Variants).ThenInclude(v => v.OptionValues)
            .AsSplitQuery(); // This prevents cartesian explosion by splitting into multiple queries
        
        var product = await query.FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        // Track product view for the recommendation system
        if (_signInManager.IsSignedIn(User))
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email!);
                if (user != null)
                {
                    await _interactionService.TrackInteractionAsync(
                        user.Id, id, InteractionType.View);
                }
            }
            catch { /* Non-critical: don't fail the page load */ }
        }

        if (product.ProductKind == ProductKind.Variable && product.Variants.Any())
        {
            var defaultVariant = product.Variants.OrderBy(v => v.Price).FirstOrDefault(); 
            if (defaultVariant != null)
            {
                // --- UPDATE THIS BLOCK ---
                DefaultVariant = defaultVariant; // Store the whole variant
                DefaultSelectedValueIds = defaultVariant.OptionValues.Select(ov => ov.Id).ToHashSet();
                
                // Check if *this specific variant* is in the cart
                DefaultVariantCartItem = Cart?.Items.FirstOrDefault(i => i.ProductVariantId == defaultVariant.Id);
                // --- END OF UPDATE ---
            }
        }

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

        // Check if AI is enabled via the AzureOpenAIClientService (reads from database)
        var aiClientService = HttpContext.RequestServices
            .GetRequiredService<Infrastructure.Services.AzureOpenAIClientService>();
        IsAIEnabled = aiClientService.IsEnabled;

        // Get AI-powered recommendations only if enabled
        if (IsAIEnabled)
        {
            try
            {
                var aiRecommendedProducts = await _aiRecommendationService.GetRecommendationsAsync(id, 4);
                AIRecommendations = aiRecommendedProducts
                    .Select(p => p.ToDto())
                    .ToList();
            }
            catch
            {
                // AI recommendations failed, don't show the section
                AIRecommendations = [];
            }

            // Generate AI review summary if there are reviews
            if (product.Reviews.Count >= 2) // Only summarize if there are at least 2 reviews
            {
                try
                {
                    AIReviewSummary = await _aiService.SummarizeReviewsAsync(product.Reviews);
                }
                catch
                {
                    // AI review summary failed, don't show it
                    AIReviewSummary = null;
                }
            }
        }

        // Load related products WITHOUT variants (just for display cards)
        // This dramatically reduces the data loaded since we don't need variant details for cards
        var relatedByBrandProducts = await _unitOfWork.Repository<Product>().GetQueryable()
            .Where(p => p.ProductBrandId == product.ProductBrandId && p.Id != product.Id)
            .Include(p => p.ProductType)
            .Include(p => p.ProductBrand)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews) // For rating calculation
            .Take(4)
            .AsSplitQuery()
            .ToListAsync();
        
        RelatedByBrand = relatedByBrandProducts
            .Select(p => p.ToDto())
            .ToList();
        
        // Get related products by TYPE (without variants)
        var relatedByTypeProducts = await _unitOfWork.Repository<Product>().GetQueryable()
            .Where(p => p.ProductTypeId == product.ProductTypeId && p.Id != product.Id)
            .Include(p => p.ProductType)
            .Include(p => p.ProductBrand)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews) // For rating calculation
            .Take(4)
            .AsSplitQuery()
            .ToListAsync();
        
        RelatedByType = relatedByTypeProducts
            .Select(p => p.ToDto())
            .ToList();

        // --- Load multi-strategy recommendations for logged-in users ---
        if (_signInManager.IsSignedIn(User))
        {
            var email2 = User.FindFirstValue(ClaimTypes.Email);
            var user2 = !string.IsNullOrEmpty(email2) ? await _userManager.FindByEmailAsync(email2) : null;
            if (user2 != null)
            {
                try
                {
                    var adaptive = await _adaptiveRecommendationService
                        .GetAdaptiveRecommendationsAsync(user2.Id, count: 4);
                    AdaptiveRecommendations = adaptive.Where(p => p.Id != id).Take(4)
                        .Select(p => p.ToDto()).ToList();
                }
                catch { }

                try
                {
                    var collab = await _adaptiveRecommendationService
                        .GetCollaborativeRecommendationsAsync(user2.Id, count: 4);
                    CollaborativeRecommendations = collab.Where(p => p.Id != id).Take(4)
                        .Select(p => p.ToDto()).ToList();
                }
                catch { }

                try
                {
                    var content = await _adaptiveRecommendationService
                        .GetContentBasedRecommendationsAsync(id, count: 4);
                    ContentBasedRecommendations = content.Where(p => p.Id != id).Take(4)
                        .Select(p => p.ToDto()).ToList();
                }
                catch { }

                try
                {
                    var popular = await _adaptiveRecommendationService
                        .GetPopularProductsAsync(count: 4);
                    PopularRecommendations = popular.Where(p => p.Id != id).Take(4)
                        .Select(p => p.ToDto()).ToList();
                }
                catch { }
            }
        }

        // Set a default return URL if none is provided
        if (string.IsNullOrEmpty(ReturnUrl))
        {
            ReturnUrl = "/Products";
        }

        Cart = await _cartService.GetCartAsync();

        var canUserReview = false;

        HasPurchasedProduct = false;
        HasReviewedProduct = false;
        if (User.Identity.IsAuthenticated)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            var purchaseSpec = new BaseSpecification<Order>(
                o => o.BuyerEmail == email &&
                     o.Status == OrderStatus.PaymentReceived &&
                     o.OrderItems.Any(oi => oi.ItemOrdered.ProductId == id)
            );

            var purchaseCount = await _unitOfWork.Repository<Order>().CountAsync(purchaseSpec);

            // Set our new, more specific properties
            HasPurchasedProduct = purchaseCount > 0;
            HasReviewedProduct = product.Reviews.Any(r => r.AppUserId == user.Id);

            // The original logic remains for the ProductDto, which is fine
            canUserReview = HasPurchasedProduct && !HasReviewedProduct;
        }
        
        // --- 4. ADD THIS SEO BLOCK ---
        var settings = await _siteSettings.GetSettingsAsync();
        var storeName = settings.GetValueOrDefault("StoreName", "Store Front");

        ViewData["Title"] = $"{product.Name} - {storeName}";
        ViewData["OgType"] = "product";
        ViewData["StoreName"] = storeName;
        
        // Create a short, clean description (max 155 chars)
        var description = product.Description.Length > 155 
            ? product.Description.Substring(0, 155) + "..." 
            : product.Description;
            
        ViewData["Description"] = description.Replace("\n", " ").Replace("\r", " ");
        
        // Set the main product image for social sharing
        ViewData["ImageUrl"] = product.Images.FirstOrDefault(i => i.IsMain)?.Url ?? "";
        // --- END OF SEO BLOCK ---

        Product = product.ToDto(canUserReview);
        NewReview = new CreateReviewDto { ProductId = id };

        return Page();
    }


    public async Task<IActionResult> OnPostAddToCartJsonAsync(int productId, int? productVariantId)
    {
        var cart = await _cartService.AddItemToCartAsync(productId, productVariantId, 1);
        return new JsonResult(new { itemCount = cart.Items.Sum(i => i.Quantity) });
    }

    public async Task<IActionResult> OnPostUpdateCartJsonAsync(int productId, int? productVariantId, int quantity)
    {
        var cart = await _cartService.SetItemQuantityAsync(productId, productVariantId, quantity);
        
        int stock;
        if (productVariantId.HasValue)
        {
            var variant = await _unitOfWork.Repository<ProductVariant>().GetByIdAsync(productVariantId.Value);
            stock = variant?.QuantityInStock ?? 0;
        }
        else
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            stock = product?.QuantityInStock ?? 0;
        }

        var updatedItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == productVariantId);
        
        return new JsonResult(new
        {
            itemCount = cart.Items.Sum(i => i.Quantity),
            newQuantity = updatedItem?.Quantity ?? 0,
            stock // Return the correct stock level for this item/variant
        });
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

    public async Task<IActionResult> OnPostSubmitReviewAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return await OnGetAsync(id);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var review = new ProductReview
        {
            ProductId = id,
            AppUserId = user.Id,
            ReviewerName = user.FirstName ?? user.UserName,
            Rating = NewReview.Rating,
            Comment = NewReview.Comment
        };

        _unitOfWork.Repository<ProductReview>().Add(review);
        await _unitOfWork.Complete();

        return RedirectToPage(new { id });
    }
}