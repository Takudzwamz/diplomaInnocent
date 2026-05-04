using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IWishlistService
    {
        /// <summary>
        /// Gets a user's wishlist, creating one if it doesn't exist.
        /// </summary>
        Task<Wishlist> GetOrCreateWishlistForUserAsync(string userId);

        /// <summary>
        /// Adds a product to a user's wishlist.
        /// </summary>
        Task AddItemToWishlistAsync(string userId, int productId);

        /// <summary>
        /// Removes an item from a user's wishlist.
        /// </summary>
        Task RemoveItemFromWishlistAsync(int wishlistItemId);
        
        /// <summary>
        /// Checks if a specific product is already in the user's wishlist.
        /// </summary>
        Task<bool> IsItemInWishlistAsync(string userId, int productId);
    }
}