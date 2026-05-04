using System;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Infrastructure.Config;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class StoreContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductOption> ProductOptions { get; set; }
    public DbSet<ProductOptionValue> ProductOptionValues { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<DeliveryMethod> DeliveryMethods { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<TrackingEvent> TrackingEvents { get; set; }

    public DbSet<ProductBrand> ProductBrands { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<ContentBlock> ContentBlocks { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }

    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<CouponProduct> CouponProducts { get; set; }
    public DbSet<HeroSlide> HeroSlides { get; set; }
    
    public DbSet<EmailTemplate> EmailTemplates { get; set; }

    public DbSet<SiteSetting> SiteSettings { get; set; }

    public DbSet<FaqItem> FaqItems { get; set; }

    // Recommendation system entities
    public DbSet<UserInteraction> UserInteractions { get; set; }
    public DbSet<ABTestExperiment> ABTestExperiments { get; set; }
    public DbSet<ABTestAssignment> ABTestAssignments { get; set; }
    public DbSet<RecommendationEvent> RecommendationEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CouponProduct>()
            .HasKey(cp => new { cp.CouponId, cp.ProductId });
            
        // --- FIX FOR MULTIPLE CASCADE PATHS ---

        // Deleting a Product will also delete its Variants.
        modelBuilder.Entity<ProductVariant>()
            .HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Deleting a Product will also delete its Images.
        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Deleting an Image will set the Variant's ImageId to null, not delete the variant.
        // This is crucial to break the cascade cycle.
        modelBuilder.Entity<ProductVariant>()
            .HasOne(v => v.Image)
            .WithMany()
            .HasForeignKey(v => v.ImageId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- END OF FIX ---
        
        // Many-to-many: Product <-> ProductOption
        modelBuilder.Entity<Product>()
            .HasMany(p => p.Options)
            .WithMany(o => o.Products)
            .UsingEntity(j => j.ToTable("ProductProductOptions"));

        // Many-to-many: ProductVariant <-> ProductOptionValue
        modelBuilder.Entity<ProductVariant>()
            .HasMany(v => v.OptionValues)
            .WithMany(ov => ov.Variants)
            .UsingEntity(j => j.ToTable("ProductVariantOptionValues"));
            
        // Recommendation system configuration
        modelBuilder.Entity<UserInteraction>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.ProductId, e.Timestamp });
            entity.HasIndex(e => e.SessionId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ABTestExperiment>(entity =>
        {
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<ABTestAssignment>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ExperimentId }).IsUnique();

            entity.HasOne(e => e.Experiment)
                .WithMany(exp => exp.Assignments)
                .HasForeignKey(e => e.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecommendationEvent>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.ExperimentId, e.EventType });
            entity.HasIndex(e => e.Strategy);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RecommendedProduct)
                .WithMany()
                .HasForeignKey(e => e.RecommendedProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Experiment)
                .WithMany()
                .HasForeignKey(e => e.ExperimentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);
    }
}
