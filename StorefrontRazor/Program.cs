using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StorefrontRazor.Services;
using StorefrontRazor.Extensions;
using MailerSend.AspNetCore;
using System.Globalization;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 1. Add Services to the Container
// =================================================================
builder.Services.AddHttpClient();
// This is the core service for a Razor Pages application.
builder.Services.AddRazorPages(options =>
{
    // Require authentication for the /Admin folder.
    options.Conventions.AuthorizeFolder("/Admin", "Admin");
});

// Set Russian culture
var ruCulture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = ruCulture;
CultureInfo.DefaultThreadCurrentUICulture = ruCulture;

// Configure Kestrel for larger file uploads (e.g., product images).
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

// Configure Redis and Data Protection for robust key management.
// This is excellent for security and scalability.
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new Exception("Redis connection string not found.");
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("skinet-2025");

// Register your DbContext (from the Infrastructure project).
builder.Services.AddDbContext<StoreContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register your services and repositories from Core/Infrastructure.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderFinalizationService, OrderFinalizationService>();
builder.Services.AddScoped<IPaymentGateway, PaystackGatewayService>();
builder.Services.AddScoped<IPaymentGateway, PayFastGatewayService>();
builder.Services.AddScoped<ICouponService, CouponService>(provider =>
{
    var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
    var userManager = provider.GetRequiredService<UserManager<AppUser>>();
    return new CouponService(unitOfWork, userManager);
});
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddSingleton<IEmailQueueService, EmailQueueService>();
builder.Services.AddHostedService<EmailSenderBackgroundService>();
builder.Services.AddSingleton<IResponseCacheService, ResponseCacheService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<IIndexNowService, IndexNowService>();
builder.Services.AddSingleton<AzureOpenAIClientService>(); // Singleton: created once at startup
builder.Services.AddScoped<IAIRecommendationService, AIRecommendationService>();
builder.Services.AddScoped<IAIService, AIService>(); // AI service for chat, summarization, and search
builder.Services.AddScoped<IAdminAIService, AdminAIService>(); // Admin AI service for insights and forecasting
builder.Services.AddScoped<IProductEmbeddingService, ProductEmbeddingService>();
// Adaptive recommendation system services
builder.Services.AddScoped<IUserInteractionService, UserInteractionService>();
builder.Services.AddScoped<IAdaptiveRecommendationService, AdaptiveRecommendationService>();
builder.Services.AddScoped<IABTestService, ABTestService>();
builder.Services.AddScoped<IRecommendationMetricsService, RecommendationMetricsService>();
builder.Services.AddScoped<IOfflineMetricsService, OfflineMetricsService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    // You can set a timeout for how long the session data should be stored
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Configure Cloudinary settings.
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Configure ASP.NET Core Identity.
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<StoreContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<StorefrontRazor.Extensions.RussianIdentityErrorDescriber>(); // For password reset tokens.

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

// Configure the application's authentication cookie.
// This is the correct setup for a server-rendered web app.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Secure default for web apps.
});


// Configure rate limiting for API endpoints (especially AI chat)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // AI chat: 10 requests per minute per IP
    options.AddPolicy("AiChat", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));

    // General API: 60 requests per minute per IP
    options.AddPolicy("GeneralApi", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            }));
});

// Default number/currency formatting (can be overridden by request culture)
var defaultCulture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // These lines are crucial for ngrok and other non-standard proxies.
    // In a production environment with a known proxy, you would configure
    // options.KnownProxies and options.KnownNetworks instead.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


// =================================================================
// 2. Configure the HTTP Request Pipeline (Middleware)
// =================================================================

var app = builder.Build();

// Generate embeddings for products on startup (only if AI is enabled and embeddings are missing)
using (var scope = app.Services.CreateScope())
{
    var embeddingService = scope.ServiceProvider.GetRequiredService<IProductEmbeddingService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Checking for products without embeddings...");
        await embeddingService.GenerateMissingEmbeddingsAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating product embeddings on startup");
    }
}

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    // Use the developer exception page for detailed errors in development.
    app.UseDeveloperExceptionPage();
}
else
{
    // Use a user-friendly error page and HSTS in production.
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enables serving files from wwwroot (CSS, JS, images).

app.UseRouting();



app.UseSession(); // Enables session state for storing user data across requests.

// These two must be placed between UseRouting() and MapRazorPages().
app.UseAuthentication(); // Checks the cookie to see who the user is.
app.UseAuthorization();  // Checks if the user is allowed to access a page.

app.UseRateLimiter(); // Apply rate limiting middleware

// Map all API endpoints (AI chat, products, cart, orders) - for chat widget and future mobile app
app.MapApiEndpoints();

// This maps URLs to your Razor Pages.
app.MapRazorPages();

// Seed the database on startup.
try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StoreContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await context.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context, userManager, roleManager);
    await RecommendationDataSeeder.SeedRecommendationDataAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database migration or seeding.");
}

app.Run();