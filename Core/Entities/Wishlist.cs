using System.Collections.Generic;

namespace Core.Entities
{
    public class Wishlist : BaseEntity
    {
        // This links the wishlist to a specific user.
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public List<WishlistItem> Items { get; set; } = new();
    }
}