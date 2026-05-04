

 using Core.Entities;

namespace Core.Interfaces;

public interface ICartService
{
    // The service will now find the cart ID itself
    Task<ShoppingCart> GetCartAsync();

    Task<ShoppingCart> GetCartAsync(string cartId);

    // This is a more logical method for adding items
    Task<ShoppingCart> AddItemToCartAsync(int productId, int? productVariantId, int quantity = 1);

    Task<ShoppingCart> SetItemQuantityAsync(int productId, int? productVariantId, int quantity);

    // Renamed for clarity
    Task<ShoppingCart> UpdateCartAsync(ShoppingCart cart);

    // No longer needs a key
    Task DeleteCartAsync();

    Task<bool> ApplyCouponAsync(string couponCode, string userEmail);
    Task RemoveCouponAsync();
    // --- Methods for the API / Mobile App (uses an explicit ID) ---

    Task<bool> DeleteCartAsync(string cartId); // Re-added for the API
}