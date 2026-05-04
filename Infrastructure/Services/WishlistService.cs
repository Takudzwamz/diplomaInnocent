using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Wishlist> GetOrCreateWishlistForUserAsync(string userId)
        {
            var wishlistRepo = _unitOfWork.Repository<Wishlist>();
            
            var spec = new BaseSpecification<Wishlist>(w => w.AppUserId == userId);
            spec.AddInclude($"{nameof(Wishlist.Items)}.{nameof(WishlistItem.Product)}.{nameof(Product.Images)}");

            var wishlist = await wishlistRepo.GetEntityWithSpec(spec);

            if (wishlist == null)
            {
                wishlist = new Wishlist { AppUserId = userId };
                wishlistRepo.Add(wishlist);
                await _unitOfWork.Complete();
            }

            return wishlist;
        }

        public async Task AddItemToWishlistAsync(string userId, int productId)
        {
            var wishlist = await GetOrCreateWishlistForUserAsync(userId);
            
            // Prevent adding duplicate items
            if (!wishlist.Items.Any(item => item.ProductId == productId))
            {
                var wishlistItem = new WishlistItem { ProductId = productId, WishlistId = wishlist.Id };
                _unitOfWork.Repository<WishlistItem>().Add(wishlistItem);
                await _unitOfWork.Complete();
            }
        }

        public async Task RemoveItemFromWishlistAsync(int wishlistItemId)
        {
            var item = await _unitOfWork.Repository<WishlistItem>().GetByIdAsync(wishlistItemId);
            if (item != null)
            {
                _unitOfWork.Repository<WishlistItem>().Remove(item);
                await _unitOfWork.Complete();
            }
        }

        public async Task<bool> IsItemInWishlistAsync(string userId, int productId)
        {
            var wishlist = await _unitOfWork.Repository<Wishlist>()
                .GetEntityWithSpec(new BaseSpecification<Wishlist>(w => w.AppUserId == userId));

            if (wishlist == null) return false;

            return await _unitOfWork.Repository<WishlistItem>()
                .GetQueryable()
                .AnyAsync(i => i.WishlistId == wishlist.Id && i.ProductId == productId);
        }
    }
}