using System.Text.Json;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CartService : ICartService
{
    private const string CartCookieKey = "StorefrontCartId";
    private readonly IDatabase _database;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICouponService _couponService;
    private readonly Infrastructure.Data.StoreContext _context;

    public CartService(IConnectionMultiplexer redis, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, ICouponService couponService, Infrastructure.Data.StoreContext context)
    {
        _database = redis.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _couponService = couponService;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    // Helper method to get the user's unique cart ID from a cookie
    private string GetOrCreateCartId()
    {
        var request = _httpContextAccessor.HttpContext!.Request;
        var response = _httpContextAccessor.HttpContext!.Response;

        var cartId = request.Cookies[CartCookieKey];

        if (string.IsNullOrEmpty(cartId))
        {
            cartId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddDays(30) };
            response.Cookies.Append(CartCookieKey, cartId, cookieOptions);
        }
        return cartId;
    }


    public async Task<ShoppingCart> GetCartAsync(string cartId)
    {
        return await GetCartByIdAsync(cartId);
    }

    // --- For the Razor Pages web app ---
    public async Task<ShoppingCart> GetCartAsync()
    {
        var cartId = GetOrCreateCartIdFromCookie();
        return await GetCartByIdAsync(cartId);
    }

    private async Task<ShoppingCart> GetCartByIdAsync(string cartId)
    {
        var data = await _database.StringGetAsync(cartId);
        if (data.IsNullOrEmpty) return new ShoppingCart { Id = cartId };
        var cart = JsonSerializer.Deserialize<ShoppingCart>(data.ToString());
        if (cart != null) cart.Id = cartId;
        return cart ?? new ShoppingCart { Id = cartId };
    }

    private string GetOrCreateCartIdFromCookie()
    {
        var request = _httpContextAccessor.HttpContext!.Request;
        var response = _httpContextAccessor.HttpContext!.Response;
        var cartId = request.Cookies[CartCookieKey];
        if (string.IsNullOrEmpty(cartId))
        {
            cartId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddDays(30) };
            response.Cookies.Append(CartCookieKey, cartId, cookieOptions);
        }
        return cartId;
    }

    public async Task<ShoppingCart> AddItemToCartAsync(int productId, int? productVariantId, int quantity = 1)
    {
        var cart = await GetCartAsync();
        Product product; // Will hold the parent product for coupon checks

        if (productVariantId.HasValue)
        {
            // --- Handle Product Variant ---
            var variantSpec = new BaseSpecification<ProductVariant>(v => v.Id == productVariantId.Value);
            // Include all necessary related data for creating the cart item and checking coupons
            variantSpec.AddInclude("Product.ProductBrand");
            variantSpec.AddInclude("Product.ProductType");
            variantSpec.AddInclude("Product.Category");
            variantSpec.AddInclude("Product.Images");
            variantSpec.AddInclude("Product.Coupons.Coupon"); // Crucial for coupon logic
            variantSpec.AddInclude("OptionValues.ProductOption");
            variantSpec.AddInclude(v => v.Image!);

            var variant = await _unitOfWork.Repository<ProductVariant>().GetEntityWithSpec(variantSpec);
            if (variant == null) return cart; // Variant not found

            product = variant.Product; // Assign the parent product for coupon logic later

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == productVariantId.Value);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = variant.ProductId,
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product.Name,
                    Price = variant.Price,
                    Quantity = quantity,
                    PictureUrl = variant.Image?.Url ?? variant.Product.PictureUrl,
                    ProductBrand = variant.Product.ProductBrand.Name,
                    ProductType = variant.Product.ProductType.Name,
                    ProductCategory = variant.Product.Category.Name,
                    SelectedOptions = string.Join(", ", variant.OptionValues.Select(ov => $"{ov.ProductOption.Name}: {ov.Name}"))
                });
            }
        }
        else
        {
            // --- Handle Simple Product ---
            var productSpec = new ProductSpecification(productId, withImages: true); // This spec already includes coupons
            var simpleProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpec(productSpec);
            if (simpleProduct == null) return cart;

            product = simpleProduct; // Assign the simple product for coupon logic later

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == null);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductVariantId = null,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    PictureUrl = product.PictureUrl,
                    ProductBrand = product.ProductBrand.Name,
                    ProductType = product.ProductType.Name,
                    ProductCategory = product.Category.Name,
                    SelectedOptions = null
                });
            }
        }

        // --- UNIFIED COUPON LOGIC ---
        // This block now runs for both simple products and variants, using the parent `product`.
        AppCoupon? productCouponDto = null;
        if (product.Coupons.Any())
        {
            var now = DateTime.UtcNow;
            var validProductCoupon = product.Coupons
                .Select(cp => cp.Coupon)
                .FirstOrDefault(c => c.IsActive &&
                               (!c.ValidFrom.HasValue || c.ValidFrom.Value <= now) &&
                               (!c.ValidUntil.HasValue || c.ValidUntil.Value >= now));

            if (validProductCoupon != null)
            {
                var productIdsForCoupon = await _context.CouponProducts
                    .Where(cp => cp.CouponId == validProductCoupon.Id)
                    .Select(cp => cp.ProductId)
                    .ToListAsync();

                productCouponDto = new AppCoupon
                {
                    Name = validProductCoupon.Description,
                    AmountOff = validProductCoupon.AmountOff,
                    PercentOff = validProductCoupon.PercentOff,
                    PromotionCode = validProductCoupon.Code,
                    CouponId = validProductCoupon.Id.ToString(),
                    ApplicableProductIds = productIdsForCoupon
                };
            }
        }

        // Conflict resolution logic
        if (productCouponDto != null)
        {
            if (cart.Coupon == null)
            {
                cart.Coupon = productCouponDto;
            }
            else
            {
                // You will need to add the `CalculateDiscount` helper method to this service if it's not already there.
                var existingDiscount = CalculateDiscount(cart.Coupon, cart.Items);
                var potentialNewDiscount = CalculateDiscount(productCouponDto, cart.Items);
                if (potentialNewDiscount > existingDiscount)
                {
                    cart.Coupon = productCouponDto;
                }
            }
        }

        return await UpdateCartAsync(cart);
    }

    private decimal CalculateDiscount(AppCoupon coupon, List<CartItem> items)
    {
        var subtotal = items.Sum(i => i.Price * i.Quantity);
        var discount = 0m;

        var eligibleItems = coupon.ApplicableProductIds.Any()
            ? items.Where(item => coupon.ApplicableProductIds.Contains(item.ProductId)).ToList()
            : items.ToList();

        var eligibleSubtotal = eligibleItems.Sum(i => i.Price * i.Quantity);

        if (coupon.AmountOff.HasValue)
        {
            discount = Math.Min(coupon.AmountOff.Value, eligibleSubtotal);
        }
        else if (coupon.PercentOff.HasValue)
        {
            discount = eligibleSubtotal * (coupon.PercentOff.Value / 100);
        }
        return discount;
    }


    public async Task<ShoppingCart> SetItemQuantityAsync(int productId, int? productVariantId, int quantity)
    {
        var cart = await GetCartAsync();
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == productVariantId);

        if (existingItem != null)
        {
            if (quantity > 0)
            {
                existingItem.Quantity = quantity;
            }
            else
            {
                cart.Items.Remove(existingItem);
            }
        }

        return await UpdateCartAsync(cart);
    }

    public async Task<ShoppingCart> UpdateCartAsync(ShoppingCart cart)
    {
        // Use the ID from the cart object itself
        await _database.StringSetAsync(cart.Id, JsonSerializer.Serialize(cart), TimeSpan.FromDays(30));
        return await GetCartAsync();
    }

    public async Task<bool> DeleteCartAsync(string cartId)
    {
        return await _database.KeyDeleteAsync(cartId);
    }

    public async Task DeleteCartAsync()
    {
        var cartId = GetOrCreateCartId();
        await _database.KeyDeleteAsync(cartId);
        // Also remove the cookie
        _httpContextAccessor.HttpContext!.Response.Cookies.Delete(CartCookieKey);
    }

    public async Task<bool> ApplyCouponAsync(string couponCode, string userEmail)
    {
        var cart = await GetCartAsync();
        if (cart == null) return false;

        // 1. Get the coupon details from our coupon service
        var appCoupon = await _couponService.GetCouponFromPromoCode(couponCode, userEmail);

        // 2. If the coupon is invalid for any reason (expired, inactive, etc.), fail.
        if (appCoupon == null)
        {
            return false;
        }

        // --- START OF NEW VALIDATION LOGIC ---
        // 3. If this coupon is product-specific...
        if (appCoupon.ApplicableProductIds.Any())
        {
            // ...check if the cart contains AT LEAST ONE of the applicable products.
            var cartProductIds = cart.Items.Select(i => i.ProductId).ToHashSet();
            if (!appCoupon.ApplicableProductIds.Any(id => cartProductIds.Contains(id)))
            {
                // If there are no eligible products in the cart, the coupon is invalid for this cart.
                return false;
            }
        }
        // --- END OF NEW VALIDATION LOGIC ---

        // 4. If all checks pass, apply the coupon to the cart and save.
        cart.Coupon = appCoupon;
        await UpdateCartAsync(cart);
        return true;
    }

    public async Task RemoveCouponAsync()
    {
        var cart = await GetCartAsync();
        if (cart.Coupon != null)
        {
            cart.Coupon = null;
            await UpdateCartAsync(cart);
        }
    }
}