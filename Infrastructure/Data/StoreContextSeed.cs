using System.Reflection;
using System.Text.Json;
using Core.DTOs;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class StoreContextSeed
{
    public static async Task SeedAsync(StoreContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // --- 1. Seed Roles and Admin User (Updated and Robust) ---
        const string adminRole = "Admin";
        const string adminEmail = "admin@test.com";
        const string adminPassword = "Pa$$w0rd";

        // Step A: Ensure the "Admin" role exists
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        // Step B: Find the admin user by email
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        // Step C: If the user doesn't exist, create them with a name and surname
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",      // <-- Name added
                LastName = "User",        // <-- Surname added
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, adminPassword);
        }

        // Step D: Ensure the user is in the "Admin" role
        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }

        // Step E: If the user exists, check and reset their password if it's incorrect
        if (!await userManager.CheckPasswordAsync(adminUser, adminPassword))
        {
            await userManager.RemovePasswordAsync(adminUser);
            await userManager.AddPasswordAsync(adminUser, adminPassword);
        }

        // --- 1b. Seed Test Shoppers for Recommendation System A/B Testing ---
        var testUsers = new[]
        {
            ("Анна",    "Иванова",    "anna@test.com"),
            ("Борис",   "Петров",     "boris@test.com"),
            ("Вера",    "Сидорова",   "vera@test.com"),
            ("Григорий","Козлов",     "grigory@test.com"),
            ("Дарья",   "Новикова",   "darya@test.com"),
            ("Евгений", "Морозов",    "evgeny@test.com"),
            ("Жанна",   "Волкова",    "zhanna@test.com"),
            ("Захар",   "Соловьёв",   "zakhar@test.com"),
            ("Ирина",   "Лебедева",   "irina@test.com"),
            ("Кирилл",  "Козлов",     "kirill@test.com"),
            ("Лариса",  "Николаева",  "larisa@test.com"),
            ("Максим",  "Фёдоров",    "maxim@test.com"),
            ("Наталья", "Смирнова",   "natalya@test.com"),
            ("Олег",    "Попов",      "oleg@test.com"),
            ("Полина",  "Зайцева",    "polina@test.com"),
            ("Роман",   "Егоров",     "roman@test.com"),
            ("Светлана","Медведева",   "svetlana@test.com"),
            ("Тимур",   "Орлов",      "timur@test.com"),
            ("Ульяна",  "Кузнецова",  "ulyana@test.com"),
        };

        var addressCities = new[] { "Москва", "Санкт-Петербург", "Казань", "Новосибирск", "Екатеринбург",
            "Нижний Новгород", "Самара", "Ростов-на-Дону", "Краснодар", "Воронеж", "Пермь", "Волгоград",
            "Челябинск", "Омск", "Уфа", "Красноярск", "Тюмень", "Иркутск", "Томск" };
        var addressStreets = new[] { "ул. Ленина", "пр. Мира", "ул. Пушкина", "ул. Гагарина", "пр. Победы",
            "ул. Кирова", "ул. Советская", "Невский пр.", "ул. Чехова", "ул. Толстого", "бульвар Мира",
            "пер. Садовый", "ул. Горького", "пр. Науки", "ул. Комсомольская", "ул. Октябрьская",
            "ул. Строителей", "ул. Молодёжная", "ул. Школьная" };
        var addrRng = new Random(123);

        for (int idx = 0; idx < testUsers.Length; idx++)
        {
            var (firstName, lastName, email) = testUsers[idx];
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    Address = new Address
                    {
                        Line1 = $"{addressStreets[idx % addressStreets.Length]}, д. {addrRng.Next(1, 120)}, кв. {addrRng.Next(1, 250)}",
                        City = addressCities[idx % addressCities.Length],
                        State = "Россия",
                        PostalCode = $"{addrRng.Next(100000, 700000)}",
                        Country = "RU",
                        PhoneNumber = $"+7{addrRng.Next(900, 999)}{addrRng.Next(1000000, 9999999)}"
                    }
                };
                await userManager.CreateAsync(user, "Pa$$w0rd");
            }
            else if (existingUser.Address == null)
            {
                // Backfill address for users already created without one
                existingUser.Address = new Address
                {
                    Line1 = $"{addressStreets[idx % addressStreets.Length]}, д. {addrRng.Next(1, 120)}, кв. {addrRng.Next(1, 250)}",
                    City = addressCities[idx % addressCities.Length],
                    State = "Россия",
                    PostalCode = $"{addrRng.Next(100000, 700000)}",
                    Country = "RU",
                    PhoneNumber = $"+7{addrRng.Next(900, 999)}{addrRng.Next(1000000, 9999999)}"
                };
                await userManager.UpdateAsync(existingUser);
            }
        }


        // --- 2. Seed Brands (Prerequisite for Products) ---
        if (!await context.ProductBrands.AnyAsync())
        {
            var brandsData = await File.ReadAllTextAsync(path + @"/Data/SeedData/brands.json");
            var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);
            if (brands != null)
            {
                context.ProductBrands.AddRange(brands);
                await context.SaveChangesAsync();
            }
        }

        // --- 3. Seed Types (Prerequisite for Products) ---
        if (!await context.ProductTypes.AnyAsync())
        {
            var typesData = await File.ReadAllTextAsync(path + @"/Data/SeedData/types.json");
            var types = JsonSerializer.Deserialize<List<ProductType>>(typesData);
            if (types != null)
            {
                context.ProductTypes.AddRange(types);
                await context.SaveChangesAsync();
            }
        }

        // --- 3.5 Seed Categories (Prerequisite for Products) ---
        if (!await context.Categories.AnyAsync())
        {
            var categoriesData = await File.ReadAllTextAsync(path + @"/Data/SeedData/categories.json");
            var categories = JsonSerializer.Deserialize<List<Category>>(categoriesData);
            if (categories != null)
            {
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
        }

        // --- 3.6 NEW: Seed Product Options (Must happen before products) ---
        if (!await context.ProductOptions.AnyAsync())
        {
            var optionsData = await File.ReadAllTextAsync(path + @"/Data/SeedData/options.json");
            var optionDtos = JsonSerializer.Deserialize<List<ProductOptionSeedDto>>(optionsData);
            if (optionDtos != null)
            {
                var options = optionDtos.Select(dto => new ProductOption
                {
                    Name = dto.Name,
                    Values = dto.Values.Select(v => new ProductOptionValue { Name = v.Name }).ToList()
                });
                context.ProductOptions.AddRange(options);
                await context.SaveChangesAsync();
            }
        }


        // --- 4. UPDATED: Seed Products (Handles both Simple and Variable) ---
        if (!await context.Products.AnyAsync())
        {
            var productsToAdd = new List<Product>();

            // Dictionaries for quick lookups
            var brandsDict = await context.ProductBrands.ToDictionaryAsync(b => b.Name, b => b.Id);
            var typesDict = await context.ProductTypes.ToDictionaryAsync(t => t.Name, t => t.Id);
            var categoriesDict = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

            // --- A. Seed Simple Products (from old file) ---
            var simpleProductsData = await File.ReadAllTextAsync(path + @"/Data/SeedData/products.json");
            var simpleProductDtos = JsonSerializer.Deserialize<List<ProductSeedDto>>(simpleProductsData);

            if (simpleProductDtos != null)
            {
                foreach (var dto in simpleProductDtos)
                {
                    var product = new Product
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        Price = dto.Price,
                        QuantityInStock = dto.QuantityInStock,
                        Images = dto.Images ?? new List<ProductImage>(),
                        ProductKind = ProductKind.Simple, // Explicitly set as Simple
                        ProductBrandId = brandsDict[dto.Brand],
                        ProductTypeId = typesDict[dto.Type],
                        CategoryId = categoriesDict[dto.Category]
                    };
                    productsToAdd.Add(product);
                }
            }

            // --- B. Seed Variable Products (from new file) ---
            var optionsDict = await context.ProductOptions.Include(o => o.Values).ToDictionaryAsync(o => o.Name);

            var variableProductsData = await File.ReadAllTextAsync(path + @"/Data/SeedData/variable-products.json");
            var variableProductDtos = JsonSerializer.Deserialize<List<ProductVariableSeedDto>>(variableProductsData);

            if (variableProductDtos != null)
            {
                foreach (var dto in variableProductDtos)
                {
                    var product = new Product
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        Price = dto.Price, // This is the base/display price
                        ProductKind = ProductKind.Variable,
                        ProductBrandId = brandsDict[dto.Brand],
                        ProductTypeId = typesDict[dto.Type],
                        CategoryId = categoriesDict[dto.Category],
                        Options = dto.Options.Select(optName => optionsDict[optName]).ToList()
                    };

                    int imageIndex = 0;
                    foreach (var variantDto in dto.Variants)
                    {
                        var newImage = new ProductImage { Url = variantDto.ImageUrl, IsMain = (imageIndex == 0) };
                        product.Images.Add(newImage);

                        var variant = new ProductVariant
                        {
                            Product = product,
                            Price = variantDto.Price,
                            QuantityInStock = variantDto.QuantityInStock,
                            Image = newImage, // Link variant to its specific image
                            OptionValues = variantDto.OptionValues.Select(valName =>
                                product.Options.SelectMany(o => o.Values).First(v => v.Name == valName)
                            ).ToList()
                        };
                        product.Variants.Add(variant);
                        imageIndex++;
                    }
                    productsToAdd.Add(product);
                }
            }

            if (productsToAdd.Any())
            {
                context.Products.AddRange(productsToAdd);
                await context.SaveChangesAsync();
            }
        }

        // --- 5. Seed Delivery Methods ---
        if (!await context.DeliveryMethods.AnyAsync())
        {
            var dmData = await File.ReadAllTextAsync(path + @"/Data/SeedData/delivery.json");
            var methods = JsonSerializer.Deserialize<List<DeliveryMethod>>(dmData);
            if (methods != null)
            {
                context.DeliveryMethods.AddRange(methods);
                await context.SaveChangesAsync();
            }
        }

        // --- 6. Seed Comprehensive Test Orders (COD) ---
        if (!await context.Orders.AnyAsync())
        {
            var rng = new Random(42);
            var products = await context.Products.Include(p => p.Images).ToListAsync();
            var deliveryMethods = await context.DeliveryMethods.ToListAsync();
            var users = await userManager.Users.Where(u => u.Email != "admin@test.com").ToListAsync();

            var russianCities = new[] { "Москва", "Санкт-Петербург", "Казань", "Новосибирск", "Екатеринбург",
                "Нижний Новгород", "Самара", "Ростов-на-Дону", "Краснодар", "Воронеж", "Пермь", "Волгоград" };
            var russianStreets = new[] { "ул. Ленина", "пр. Мира", "ул. Пушкина", "ул. Гагарина", "пр. Победы",
                "ул. Кирова", "ул. Советская", "Невский пр.", "ул. Чехова", "ул. Толстого", "бульвар Мира", "пер. Садовый" };

            // 75 orders spread over 60 days — realistic distribution
            var totalOrders = 75;
            for (int i = 0; i < totalOrders; i++)
            {
                var user = users[rng.Next(users.Count)];
                var numItems = rng.Next(1, 5); // 1-4 items per order
                var selectedProducts = products.OrderBy(_ => rng.Next()).Take(numItems).ToList();
                var deliveryMethod = deliveryMethods[rng.Next(deliveryMethods.Count)];

                var orderItems = selectedProducts.Select(p => new OrderItem
                {
                    ItemOrdered = new ProductItemOrdered
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        PictureUrl = p.Images.FirstOrDefault()?.Url ?? "/images/placeholder.png"
                    },
                    Price = p.Price,
                    Quantity = rng.Next(1, 4)
                }).ToList();

                var subtotal = orderItems.Sum(oi => oi.Price * oi.Quantity);

                // Apply discount to ~20% of orders
                var discount = 0m;
                string? couponCode = null;
                if (rng.Next(100) < 20)
                {
                    discount = Math.Round(subtotal * (rng.Next(5, 20) / 100m), 2);
                    couponCode = rng.Next(2) == 0 ? "WELCOME10" : "SALE15";
                }

                // Order date: spread over 60 days, more recent orders more likely
                var daysAgo = (int)(Math.Pow(rng.NextDouble(), 1.5) * 60); // Bias toward recent
                var orderDate = DateTime.UtcNow.AddDays(-daysAgo).AddHours(rng.Next(8, 22)).AddMinutes(rng.Next(0, 60));

                // Delivery status distribution based on age
                DeliveryStatus dStatus;
                OrderStatus oStatus;
                if (daysAgo < 2)
                {
                    // Very recent — still processing
                    dStatus = DeliveryStatus.Processing;
                    oStatus = OrderStatus.PaymentReceived;
                }
                else if (daysAgo < 5)
                {
                    // Recent — shipped or processing
                    dStatus = rng.Next(3) == 0 ? DeliveryStatus.Processing : DeliveryStatus.Shipped;
                    oStatus = OrderStatus.PaymentReceived;
                }
                else if (daysAgo < 14)
                {
                    // Medium age — mostly shipped or out for delivery
                    var roll = rng.Next(10);
                    if (roll < 2) dStatus = DeliveryStatus.Shipped;
                    else if (roll < 4) dStatus = DeliveryStatus.OutForDelivery;
                    else dStatus = DeliveryStatus.Delivered;
                    oStatus = OrderStatus.PaymentReceived;
                }
                else
                {
                    // Old orders — delivered (with small % refunded)
                    if (rng.Next(100) < 8)
                    {
                        dStatus = DeliveryStatus.Delivered;
                        oStatus = OrderStatus.Refunded;
                    }
                    else
                    {
                        dStatus = DeliveryStatus.Delivered;
                        oStatus = OrderStatus.PaymentReceived;
                    }
                }

                var order = new Order
                {
                    BuyerEmail = user.Email!,
                    ShippingAddress = new ShippingAddress
                    {
                        Name = user.FirstName ?? "Тест",
                        LastName = user.LastName ?? "Пользователь",
                        Line1 = $"{russianStreets[rng.Next(russianStreets.Length)]}, д. {rng.Next(1, 150)}, кв. {rng.Next(1, 300)}",
                        City = russianCities[rng.Next(russianCities.Length)],
                        State = "Россия",
                        PostalCode = $"{rng.Next(100000, 700000)}",
                        Country = "RU",
                        PhoneNumber = $"+7{rng.Next(900, 999)}{rng.Next(1000000, 9999999)}"
                    },
                    DeliveryMethod = deliveryMethod,
                    OrderItems = orderItems,
                    Subtotal = subtotal,
                    Discount = discount,
                    CouponCode = couponCode,
                    PaymentGatewayName = "CashOnDelivery",
                    PaymentReference = Guid.NewGuid().ToString(),
                    GatewayTransactionId = $"COD-{orderDate:yyyyMMdd}-{i + 1:D3}",
                    Status = oStatus,
                    DeliveryStatus = dStatus,
                    OrderDate = orderDate
                };

                // Tracking events based on delivery status
                order.TrackingEvents.Add(new TrackingEvent
                {
                    Status = "Processing",
                    Notes = "Заказ подтверждён. Оплата при получении.",
                    EventDate = orderDate
                });

                if (dStatus >= DeliveryStatus.Shipped)
                {
                    order.TrackingEvents.Add(new TrackingEvent
                    {
                        Status = "Shipped",
                        Notes = $"Отправлен через {deliveryMethod.ShortName}. Трек: {rng.Next(10000000, 99999999)}",
                        EventDate = orderDate.AddDays(rng.Next(1, 3))
                    });
                }
                if (dStatus >= DeliveryStatus.OutForDelivery)
                {
                    order.TrackingEvents.Add(new TrackingEvent
                    {
                        Status = "OutForDelivery",
                        Notes = "Курьер выехал к получателю.",
                        EventDate = orderDate.AddDays(rng.Next(3, 5))
                    });
                }
                if (dStatus == DeliveryStatus.Delivered)
                {
                    order.TrackingEvents.Add(new TrackingEvent
                    {
                        Status = "Delivered",
                        Notes = "Заказ доставлен. Подпись получена.",
                        EventDate = orderDate.AddDays(rng.Next(4, 8))
                    });
                }
                if (oStatus == OrderStatus.Refunded)
                {
                    order.TrackingEvents.Add(new TrackingEvent
                    {
                        Status = "Refunded",
                        Notes = "Возврат средств по запросу покупателя.",
                        EventDate = orderDate.AddDays(rng.Next(8, 14))
                    });
                }

                context.Orders.Add(order);
            }

            await context.SaveChangesAsync();
        }


        var settingsRepo = context.Set<SiteSetting>();
        var existingSettings = await settingsRepo.ToDictionaryAsync(s => s.Key, s => s.Value);

        var allSettings = new List<SiteSetting>
        {
            // General
            new() { Key = "StoreName", Value = "Devs Store" },
            new() { Key = "StoreLogoUrl", Value = "" },
            new() { Key = "StoreFaviconUrl", Value = "" },
            new() { Key = "PublicUrl", Value = "http://localhost:5106" },
            new() { Key = "AdminNotificationEmail", Value = "admin@test.com" },

            // --- ADD THIS ENTIRE NEW SECTION ---
            
            // --- General Payment Config ---
            new() { Key = "Payment_ActiveGateway", Value = "CashOnDelivery" }, // Can be "CashOnDelivery", "Paystack" or "PayFast"
            new() { Key = "Payment_SiteMode", Value = "Test" }, // Can be "Test" or "Live"

            // --- Paystack Keys ---
            new() { Key = "Paystack_Test_SecretKey", Value = "sk_test_3b766126e314a05e9a2865892a4eec9f86a46ae2" },
            new() { Key = "Paystack_Test_PublicKey", Value = "pk_test_9addfb2f8354301da681e4b1cc5bb945d574a314" },
            new() { Key = "Paystack_Live_SecretKey", Value = "" }, // Merchant must fill this in
            new() { Key = "Paystack_Live_PublicKey", Value = "" }, // Merchant must fill this in

            // --- PayFast Keys ---
            new() { Key = "PayFast_Test_MerchantId", Value = "10021843" }, // PayFast sandbox default
            new() { Key = "PayFast_Test_MerchantKey", Value = "9g2h0j8qqudb5" }, // PayFast sandbox default
            new() { Key = "PayFast_Test_Passphrase", Value = "storefronttest123" }, // PayFast sandbox default
            new() { Key = "PayFast_Live_MerchantId", Value = "" }, // Merchant must fill this in
            new() { Key = "PayFast_Live_MerchantKey", Value = "" }, // Merchant must fill this in
            new() { Key = "PayFast_Live_Passphrase", Value = "" }, // Merchant must fill this in

            new() { Key = "SendGrid_ApiKey", Value = "" }, // Merchant must configure SendGrid API key
            new() { Key = "Cloudinary_CloudName", Value = "dyzeuqi75" },
            new() { Key = "Cloudinary_ApiKey", Value = "857686693541222" },
            new() { Key = "Cloudinary_ApiSecret", Value = "S2JU71xXuOgvnF9XwaNAb4JUPuE" },
            new() { Key = "IndexNow_ApiKey", Value = "e8b9eacc09364383a7f6c3be8ce49fdb" }, // Merchant should set their own key

            // AI Settings - Azure OpenAI Configuration
            new() { Key = "AI_Enabled", Value = "true" },
            new() { Key = "AI_Endpoint", Value = "https://storefrontai.openai.azure.com/" }, // Merchant must configure Azure OpenAI endpoint
            new() { Key = "AI_ApiKey", Value = "" }, // Set your Azure OpenAI API key here or via Admin → Settings
            new() { Key = "AI_EmbeddingDeployment", Value = "text-embedding-ada-002" }

        };

        // Only add settings that don't already exist in the database
        var settingsToAdd = allSettings.Where(s => !existingSettings.ContainsKey(s.Key)).ToList();

        if (settingsToAdd.Any())
        {
            settingsRepo.AddRange(settingsToAdd);
            await context.SaveChangesAsync();
        }

        if (!await context.ContentBlocks.AnyAsync())
        {
            var contentBlocks = new List<ContentBlock>
            {
                new() {
                    Key = "home-page-promo-html",
                    Title = "Промо-баннер главной страницы",
                    Content = @"<div class='alert text-center border-0 shadow-sm rounded-3 py-3 mb-4' role='alert' style='background: var(--bs-info-bg-subtle); color: var(--bs-info-text-emphasis); font-size: 1.1rem;'> 🎉 Используйте код <strong class='text-primary'>СКИДКА1000</strong> и получите <span class='fw-bold text-success'>скидку 1000₽</span> на первую покупку!</div>",
                    IsHtml = true
                },
                new() {
                    Key = "return-policy",
                    Title = "Return Policy Page Content",
                    Content = """
                    <h1 class="mb-4">Our Return Policy</h1>
                    <p>We want you to shop with confidence. If for any reason you are not completely satisfied with your purchase, we’re here to help.</p>
                    <h2 class="mt-4">Returns</h2>
                    <p>
                    You have <strong>30 calendar days</strong> from the date you received your item to request a return. 
                    To be eligible, your item must be unused, in the same condition that you received it, and in the original packaging.
                    </p>
                    <h2 class="mt-4">Exchanges</h2>
                    <p>
                    We only replace items if they are defective, damaged, or if you received the wrong product. 
                    If you need an exchange, please contact our support team before sending your item back.
                    </p>
                    <h2 class="mt-4">Refunds</h2>
                    <p>
                    Once we receive your return, we will inspect it and notify you of the status. 
                    If approved, your refund will be processed to your original method of payment within 
                    <strong>5–10 business days</strong>. Shipping costs are non-refundable.
                    </p>
                    <h2 class="mt-4">Non-Returnable Items</h2>
                    <ul>
                    <li>Gift cards</li>
                    <li>Downloadable software or digital products</li>
                    <li>Perishable goods (such as food, flowers, or personal care items)</li>
                    </ul>
                    <h2 class="mt-4">How to Start a Return</h2>
                    <p>
                    To initiate a return, please email us at <a href="mailto:support@example.com">support@example.com</a> 
                    with your order number and details about the product. Our team will guide you through the process.
                    </p>
                    <p class="mt-4"><em>Note: This return policy does not affect your statutory rights.</em></p>
                    """,
                    IsHtml = true
                },
                new() {
                    Key = "contact-phone",
                    Title = "Контактный телефон",
                    Content = "+7 (495) 123-45-67",
                    IsHtml = false
                },
                new() {
                    Key = "contact-email",
                    Title = "Контактный email",
                    Content = "info@devs-store.ru",
                    IsHtml = false
                },
                new() {
                    Key = "contact-address",
                    Title = "Физический адрес",
                    Content = "г. Москва, ул. Тверская, д. 12, офис 45",
                    IsHtml = false
                },
                new() {
                    Key = "footer-about-us",
                    Title = "О нас (подвал)",
                    Content = "Предоставляем качественные товары с быстрой и надёжной доставкой. Наша цель — лучший опыт онлайн-покупок для наших клиентов.",
                    IsHtml = false
                },
                new() {
                    Key = "social-facebook-url",
                    Title = "Соцсети: VK",
                    Content = "https://vk.com/devs-store",
                    IsHtml = false
                },
                new() {
                    Key = "social-twitter-url",
                    Title = "Соцсети: Telegram",
                    Content = "https://t.me/devs_store",
                    IsHtml = false
                },
                new() {
                    Key = "social-instagram-url",
                    Title = "Соцсети: Instagram",
                    Content = "https://instagram.com/devs-store",
                    IsHtml = false
                },
                new() {
                    Key = "social-linkedin-url",
                    Title = "Соцсети: LinkedIn",
                    Content = "https://linkedin.com/company/devs-store",
                    IsHtml = false
                },
                new() {
                    Key = "social-whatsapp-url",
                    Title = "Соцсети: WhatsApp",
                    Content = "https://wa.me/74951234567",
                    IsHtml = false
                },
                new() {
                    Key = "homepage-show-brands",
                    Title = "Главная: показ брендов",
                    Content = "true",
                    IsHtml = false
                },
                new() {
                    Key = "homepage-show-categories",
                    Title = "Главная: показ категорий",
                    Content = "true",
                    IsHtml = false
                }
            };
            context.ContentBlocks.AddRange(contentBlocks);
            await context.SaveChangesAsync();
        }

        // --- START: ADD THIS NEW SECTION FOR EMAIL TEMPLATES ---
        if (!await context.EmailTemplates.AnyAsync())
        {
            var emailTemplates = new List<EmailTemplate>
            {
                new()
                {
                    Name = "Купон на скидку",
                    Subject = "Специальное предложение {DiscountValue} для вас! 🎁",
                    Body = """
                    <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;">
                        <div style="background-color: #0d6efd; color: #ffffff; padding: 20px; text-align: center;">
                            <h1 style="margin: 0; font-size: 28px;">{Headline}</h1>
                        </div>
                        <div style="padding: 30px 20px;">
                            <h2 style="color: #0d6efd;">Получите скидку {DiscountValue} на покупку!</h2>
                            <p>В благодарность за то, что вы наш постоянный клиент, мы рады предложить вам специальную скидку на следующую покупку.</p>
                            <p>Используйте следующий код при оформлении заказа:</p>
                            <div style="text-align: center; margin: 30px 0;">
                                <div style="background-color: #f8f9fa; border: 2px dashed #0d6efd; padding: 15px; display: inline-block; border-radius: 5px;">
                                    <strong style="font-size: 24px; color: #333; letter-spacing: 2px;">{CouponCode}</strong>
                                </div>
                            </div>
                            <div style="text-align: center;">
                                <a href="{ButtonLink}" style="display: inline-block; padding: 12px 24px; font-size: 16px; color: #fff; background-color: #0d6efd; text-decoration: none; border-radius: 5px;">
                                    {ButtonText}
                                </a>
                            </div>
                            <p style="margin-top: 30px;">Приятных покупок,<br><strong>Команда Devs Store</strong></p>
                        </div>
                    </div>
                    """
                },
                new()
                {
                    Name = "Новый товар",
                    Subject = "Новинка: {ProductName} уже в продаже! ✨",
                    Body = """
                    <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;">
                        <div style="background-color: #198754; color: #ffffff; padding: 20px; text-align: center;">
                            <h1 style="margin: 0; font-size: 28px;">{Headline}</h1>
                        </div>
                        <div style="padding: 30px 20px; text-align: center;">
                            <img src="{ProductImageUrl}" alt="{ProductName}" style="max-width: 100%; height: auto; border-radius: 5px; margin-bottom: 20px;">
                            <h2 style="color: #198754;">Представляем новинку: {ProductName}</h2>
                            <p style="font-size: 16px;">{ProductDescription}</p>
                            <div style="text-align: center; margin-top: 30px;">
                                <a href="{ButtonLink}" style="display: inline-block; padding: 12px 24px; font-size: 16px; color: #fff; background-color: #198754; text-decoration: none; border-radius: 5px;">
                                    Подробнее
                                </a>
                            </div>
                        </div>
                    </div>
                    """
                },
                new()
                {
                    Name = "Праздничная распродажа",
                    Subject = "Распродажа {HolidayName} началась! 🥳🥳🥳",
                    Body = """
                    <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;">
                        <div style="background-color: #dc3545; color: #ffffff; padding: 20px; text-align: center;">
                            <h1 style="margin: 0; font-size: 28px;">{Headline}</h1>
                        </div>
                        <div style="padding: 30px 20px;">
                            <h2 style="color: #dc3545;">Распродажа {HolidayName} началась!</h2>
                            <p>{SaleDetails}</p>
                            <p>Используйте промокод ниже и получите <strong>скидку {DiscountValue}</strong> на заказ.</p>
                            <div style="text-align: center; margin: 30px 0;">
                                <div style="background-color: #f8f9fa; border: 2px dashed #dc3545; padding: 15px; display: inline-block; border-radius: 5px;">
                                    <strong style="font-size: 24px; color: #333; letter-spacing: 2px;">{CouponCode}</strong>
                                </div>
                            </div>
                            <div style="text-align: center;">
                                <a href="{ButtonLink}" style="display: inline-block; padding: 12px 24px; font-size: 16px; color: #fff; background-color: #dc3545; text-decoration: none; border-radius: 5px;">
                                    Перейти к распродаже
                                </a>
                            </div>
                        </div>
                    </div>
                    """
                },
                new()
                {
                    Name = "Объявление магазина",
                    Subject = "Важное обновление от Devs Store",
                    Body = """
                    <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;">
                        <div style="background-color: #6c757d; color: #ffffff; padding: 20px; text-align: center;">
                            <h1 style="margin: 0; font-size: 28px;">{Headline}</h1>
                        </div>
                        <div style="padding: 30px 20px;">
                            <p>Здравствуйте,</p>
                            <p>{BodyText}</p>
                            <p>Если у вас есть вопросы, не стесняйтесь обращаться к нам.</p>
                            <p>С уважением,<br><strong>Команда Devs Store</strong></p>
                        </div>
                    </div>
                    """
                },
                new()
                {
                    Name = "Товар в центре внимания",
                    Subject = "В фокусе: {ProductName}",
                    Body = """
                    <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;">
                        <div style="padding: 0;">
                            <img src="{ProductImageUrl}" alt="{ProductName}" style="width: 100%; height: auto;">
                        </div>
                        <div style="padding: 30px 20px;">
                            <h2>{Headline}</h2>
                            <p>{BodyText}</p>
                            <div style="text-align: center; margin-top: 30px;">
                                <a href="{ButtonLink}" style="display: inline-block; padding: 12px 24px; font-size: 16px; color: #fff; background-color: #0d6efd; text-decoration: none; border-radius: 5px;">
                                    Узнать больше
                                </a>
                            </div>
                        </div>
                    </div>
                    """
                }
            };
            context.EmailTemplates.AddRange(emailTemplates);
            await context.SaveChangesAsync();
        }
    }
}
