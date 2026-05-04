

 using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ProductRepository(StoreContext context) : IProductRepository
{
    // UPDATED to filter by related entity name and include related data
    public async Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type, string? sort)
    {
        var query = context.Products
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.ProductBrand.Name == brand);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(p => p.ProductType.Name == type);
        }

        query = sort switch
        {
            "priceAsc" => query.OrderBy(p => p.Price),
            "priceDesc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.Name)
        };

        return await query.ToListAsync();
    }

    // UPDATED to include related data
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await context.Products
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductType)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    // UPDATED to query the ProductBrands table directly
    public async Task<IReadOnlyList<ProductBrand>> GetBrandsAsync()
    {
        return await context.ProductBrands.ToListAsync();
    }

    // UPDATED to query the ProductTypes table directly
    public async Task<IReadOnlyList<ProductType>> GetTypesAsync()
    {
        return await context.ProductTypes.ToListAsync();
    }


    // --- NO CHANGES NEEDED FOR THE METHODS BELOW ---

    public void AddProduct(Product product)
    {
        context.Products.Add(product);
    }

    public void UpdateProduct(Product product)
    {
        context.Entry(product).State = EntityState.Modified;
    }

    public void DeleteProduct(Product product)
    {
        context.Products.Remove(product);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool ProductExists(int id)
    {
        return context.Products.Any(x => x.Id == id);
    }
}