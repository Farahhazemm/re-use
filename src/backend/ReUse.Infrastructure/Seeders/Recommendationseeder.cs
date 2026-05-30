using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Reuse.Infrastructure.Identity.Models;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Seeders;

/// <summary>
/// Seeds all data required to exercise the Recommendation System.
///
/// Creation order (FK dependencies):
///   1. Identity users  (ApplicationUser)
///   2. Domain users    (User)
///   3. Products        (RegularProduct — owned by Users B/C/D/E + 2 owned by User A for exclusion)
///   4. Follows         (User A → User B)
///   5. CategoryFollows (User A → Electronics, Books)
///   6. Favorites       (User A: 3×Electronics, 2×Books, 0×Fashion)
///   7. ProductComments (0 / 3 / 10 comments on various products)
///
/// Guard: the seeder checks for the sentinel e-mail "seed.usera@reuse.dev" and
/// exits early if already present, so it is safe to call on every startup.
/// </summary>
public static class RecommendationSeeder
{
    // ──────────────────────────────────────────────────────────────────────────
    // Constants
    // ──────────────────────────────────────────────────────────────────────────

    private const string DefaultPassword = "User@123";
    private const string UserRole = "User";

    // Sentinel used for the idempotency guard
    private const string UserAEmail = "seed.usera@reuse.dev";
    private const string UserBEmail = "seed.userb@reuse.dev";
    private const string UserCEmail = "seed.userc@reuse.dev";
    private const string UserDEmail = "seed.userd@reuse.dev";
    private const string UserEEmail = "seed.usere@reuse.dev";

    // Category slugs that already exist from CategorySeeder
    private const string SlugElectronics = "electronics";        // parent
    private const string SlugPhones = "phones-accessories";      // child of Electronics
    private const string SlugLaptops = "laptops-computers";      // child of Electronics
    private const string SlugAudio = "audio";                    // child of Electronics
    private const string SlugBooks = "books";                    // child of Books & Media
    private const string SlugFashionMen = "fashion-men";         // parent (Fashion)
    private const string SlugFashionWomen = "fashion-women";     // parent (Fashion)
    private const string SlugSportsOutdoors = "sports-outdoors"; // parent (Sports)
    private const string SlugFitness = "fitness-equipment";      // child of Sports
    private const string SlugFurniture = "furniture";            // child of Home & Garden

    // ──────────────────────────────────────────────────────────────────────────
    // Seed records
    // ──────────────────────────────────────────────────────────────────────────

    private record SeedUser(
        string UserName,
        string FullName,
        string Email,
        string Bio,
        string? City,
        string? Country);

    private record SeedImage(string Url);

    private record SeedRegular(
        string CategorySlug,
        string OwnerEmail,
        string Title,
        string Description,
        ProductCondition Condition,
        string? City,
        string? Country,
        DateTime CreatedAt,
        decimal Price,
        bool AllowNegotiation,
        SeedImage[] Images);

    // ── Users ─────────────────────────────────────────────────────────────────

    private static readonly SeedUser[] RecommendationUsers =
    [
        // User A — main recommendation test user, Cairo/Egypt
        new("rec_user_a", "Layla Nasser",    UserAEmail, "Tech and books collector.",         "Cairo",      "Egypt"),
        // User B — seller, followed by User A
        new("rec_user_b", "Karim Soliman",   UserBEmail, "Electronics reseller.",             "Cairo",      "Egypt"),
        // User C — seller, different city
        new("rec_user_c", "Nour Ibrahim",    UserCEmail, "Fashion curator.",                  "Giza",       "Egypt"),
        // User D — seller, different city
        new("rec_user_d", "Tarek Ramadan",   UserDEmail, "Sports gear & furniture seller.",   "Alexandria", "Egypt"),
        // User E — cold-start (no follows, no favorites, no products)
        new("rec_user_e", "Dina Mostafa",    UserEEmail, "Just joined.",                      "Cairo",      "Egypt"),
    ];

    // ── Products ──────────────────────────────────────────────────────────────
    // Naming convention for CreatedAt offsets to verify freshness tiers:
    //   Today / -1d / -3d / -7d / -15d / -30d / -60d

    private static SeedRegular[] BuildProducts(DateTime now) =>
    [
        // ── ELECTRONICS (category affinity for User A) ────────────────────────
        // Owned by Seller B (seller affinity for User A who follows B)

        new(SlugPhones, UserBEmail,
            "iPhone 15 Pro 256GB – Natural Titanium",
            "Apple iPhone 15 Pro, 256GB, Natural Titanium. Battery 98%. Original box, cable and charger. Zero scratches.",
            ProductCondition.LikeNew, "Cairo", "Egypt",
            now,                                                    // Today → freshness 1.0
            950m, true,
            [new("https://images.unsplash.com/photo-1632661674596-df8be070a5c5?w=800")]),

        new(SlugPhones, UserBEmail,
            "iPhone 14 Pro 128GB – Deep Purple",
            "iPhone 14 Pro 128GB Deep Purple. Battery 91%. Minor wear on frame, screen pristine.",
            ProductCondition.Used, "Cairo", "Egypt",
            now.AddDays(-1),                                        // -1 day → freshness 1.0
            750m, true,
            [new("https://images.unsplash.com/photo-1591337676887-a217a6970a8a?w=800")]),

        new(SlugPhones, UserBEmail,
            "Samsung Galaxy S24 Ultra 256GB – Titanium Black",
            "Samsung Galaxy S24 Ultra, 256GB, Titanium Black. Like-new, used 3 months. All accessories.",
            ProductCondition.LikeNew, "Giza", "Egypt",
            now.AddDays(-3),                                        // -3 days → freshness 0.8
            870m, false,
            [new("https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=800")]),

        new(SlugPhones, UserCEmail,
            "iPhone Case for iPhone 15 Pro – MagSafe Compatible",
            "Brand new MagSafe leather case for iPhone 15 Pro. Midnight Blue.",
            ProductCondition.New, "Giza", "Egypt",
            now.AddDays(-3),                                        // -3 days; same category as iPhone 15 Pro → SimilarProducts
            35m, false,
            [new("https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?w=800")]),

        new(SlugLaptops, UserBEmail,
            "MacBook Air M2 13\" 8GB / 256GB – Midnight",
            "2022 MacBook Air M2, 8GB RAM, 256GB SSD. Excellent condition. No dents. Original charger.",
            ProductCondition.LikeNew, "Cairo", "Egypt",
            now.AddDays(-7),                                        // -7 days → freshness 0.6
            1050m, false,
            [new("https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=800")]),

        new(SlugAudio, UserBEmail,
            "Sony WH-1000XM5 Wireless Headphones",
            "Latest Sony XM5, black. Purchased 4 months ago. Perfect noise cancellation. Box and all accessories.",
            ProductCondition.LikeNew, "Cairo", "Egypt",
            now.AddDays(-15),                                       // -15 days → freshness ~0.75
            240m, true,
            [new("https://images.unsplash.com/photo-1583394838336-acd977736f90?w=800")]),

        new(SlugLaptops, UserCEmail,
            "Dell XPS 15 i7 / 16GB / 512GB",
            "Dell XPS 15 9500, Core i7-10750H, 16GB, 512GB NVMe. Used for office work. No overheating issues.",
            ProductCondition.Used, "Giza", "Egypt",
            now.AddDays(-30),                                       // -30 days → freshness ~0.5
            880m, true,
            [new("https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=800")]),

        new(SlugAudio, UserDEmail,
            "JBL Charge 5 Portable Bluetooth Speaker",
            "JBL Charge 5 in teal. Bought last year, used occasionally. IP67 waterproof. Includes USB-C cable.",
            ProductCondition.LikeNew, "Alexandria", "Egypt",
            now.AddDays(-60),                                       // -60 days → freshness 0.0
            120m, true,
            [new("https://images.unsplash.com/photo-1545127398-14699f92334b?w=800")]),

        // ── BOOKS (category affinity for User A) ──────────────────────────────

        new(SlugBooks, UserCEmail,
            "Clean Code by Robert C. Martin – Paperback",
            "Clean Code paperback, 2nd printing. Light reading marks in margin. Essential for any developer.",
            ProductCondition.Used, "Giza", "Egypt",
            now,                                                    // Today
            18m, false,
            [new("https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=800")]),

        new(SlugBooks, UserDEmail,
            "The Pragmatic Programmer – 20th Anniversary Edition",
            "Pragmatic Programmer 20th anniversary hardcover. Read once, spine is perfect.",
            ProductCondition.LikeNew, "Alexandria", "Egypt",
            now.AddDays(-3),
            25m, false,
            [new("https://images.unsplash.com/photo-1512820790803-83ca734da794?w=800")]),

        new(SlugBooks, UserCEmail,
            "System Design Interview Vol. 1 & 2 Bundle",
            "Both volumes of System Design Interview by Alex Xu. Lightly highlighted. Great bundle deal.",
            ProductCondition.Used, "Giza", "Egypt",
            now.AddDays(-7),
            40m, true,
            [new("https://images.unsplash.com/photo-1532012197267-da84d127e765?w=800")]),

        new(SlugBooks, UserDEmail,
            "Designing Data-Intensive Applications – O'Reilly",
            "DDIA by Martin Kleppmann, paperback. Some pencil notes in Ch. 3–5. Otherwise fine.",
            ProductCondition.Used, "Alexandria", "Egypt",
            now.AddDays(-15),
            30m, true,
            [new("https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=800")]),

        // ── FASHION (no affinity for User A → score 0 on category affinity) ───

        new(SlugFashionMen, UserCEmail,
            "Levi's 501 Original Jeans – Size 32×32",
            "Classic Levi's 501. Washed black, slim straight fit. Worn a handful of times.",
            ProductCondition.LikeNew, "Giza", "Egypt",
            now.AddDays(-1),
            55m, true,
            [new("https://images.unsplash.com/photo-1542272604-787c3835535d?w=800")]),

        new(SlugFashionWomen, UserCEmail,
            "Zara Satin Midi Skirt – Size S",
            "Zara satin midi skirt in champagne. Size S. Worn once for an event.",
            ProductCondition.LikeNew, "Giza", "Egypt",
            now.AddDays(-7),
            35m, false,
            [new("https://images.unsplash.com/photo-1595777457583-95e059d581b8?w=800")]),

        new(SlugFashionMen, UserDEmail,
            "Nike Tech Fleece Hoodie – Size L",
            "Navy Nike Tech Fleece full-zip hoodie, size L. Excellent condition, no pilling.",
            ProductCondition.LikeNew, "Alexandria", "Egypt",
            now.AddDays(-30),
            70m, false,
            [new("https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=800")]),

        // ── SPORTS & FITNESS ──────────────────────────────────────────────────

        new(SlugFitness, UserDEmail,
            "Adjustable Dumbbell Set 2–24kg – Bowflex SelectTech",
            "Bowflex SelectTech 552 adjustable dumbbells. Both bells + stand. Replaces 15 sets.",
            ProductCondition.Used, "Alexandria", "Egypt",
            now.AddDays(-1),
            320m, true,
            [new("https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=800")]),

        new(SlugSportsOutdoors, UserDEmail,
            "Kipsta Football – Size 5 (FIFA Quality)",
            "Decathlon Kipsta F900 FIFA Quality football. Used one season indoors. Good condition.",
            ProductCondition.Used, "Cairo", "Egypt",               // Cairo → location match for User A
            now.AddDays(-7),
            22m, false,
            [new("https://images.unsplash.com/photo-1570498839593-e565b39455fc?w=800")]),

        // ── FURNITURE ─────────────────────────────────────────────────────────

        new(SlugFurniture, UserDEmail,
            "IKEA KALLAX 2×4 Shelf Unit – White",
            "IKEA KALLAX 2×4 shelving unit in white. Minor scuffs on base. Easy to disassemble.",
            ProductCondition.Used, "Alexandria", "Egypt",
            now.AddDays(-3),
            75m, true,
            [new("https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=800")]),

        new(SlugFurniture, UserCEmail,
            "Ergonomic Mesh Office Chair – High Back",
            "High-back mesh office chair with lumbar support and adjustable armrests. Used 1 year.",
            ProductCondition.Used, "Giza", "Egypt",
            now.AddDays(-15),
            110m, true,
            [new("https://images.unsplash.com/photo-1580480055273-228ff5388ef8?w=800")]),

        new(SlugFurniture, UserBEmail,
            "Solid Wood Dining Table 6-Seater",
            "Solid acacia wood dining table, 180×90cm. Seats 6 comfortably. Minimal wear.",
            ProductCondition.Used, "Cairo", "Egypt",               // Cairo → location match for User A
            now.AddDays(-60),
            350m, true,
            [new("https://images.unsplash.com/photo-1533090481720-856c6e3c1fdc?w=800")]),

        // ── User A OWNED products (Exclusion Testing) ─────────────────────────
        // These must NEVER appear in User A's recommendation feed.

        new(SlugPhones, UserAEmail,
            "Xiaomi Redmi Note 12 – 128GB (My Listing)",
            "My own Xiaomi Redmi Note 12. 128GB, Onyx Gray. Battery health 95%. Selling to upgrade.",
            ProductCondition.Used, "Cairo", "Egypt",
            now.AddDays(-5),
            180m, true,
            [new("https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=800")]),

        new(SlugBooks, UserAEmail,
            "JavaScript: The Good Parts – My Copy",
            "My copy of JS: The Good Parts by Douglas Crockford. Some highlights. Selling after finishing.",
            ProductCondition.Used, "Cairo", "Egypt",
            now.AddDays(-10),
            10m, false,
            [new("https://images.unsplash.com/photo-1476275466078-4cdc71f21e2e?w=800")]),
    ];

    // ──────────────────────────────────────────────────────────────────────────
    // Entry point
    // ──────────────────────────────────────────────────────────────────────────

    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        // Idempotency guard — if User A's domain record already exists, skip.
        if (await dbContext.Set<User>().AnyAsync(u => u.Email == UserAEmail))
            return;

        // ── Step 1 & 2: Create identity + domain users ────────────────────────
        var domainUsers = await CreateUsersAsync(services);

        // ── Step 3: Create products ───────────────────────────────────────────
        var products = await CreateProductsAsync(dbContext, domainUsers);

        // ── Step 4: Follows (User A → User B) ────────────────────────────────
        await CreateFollowsAsync(dbContext, domainUsers);

        // ── Step 5: CategoryFollows (User A → Electronics, Books) ─────────────
        await CreateCategoryFollowsAsync(dbContext, domainUsers);

        // ── Step 6: Favorites ─────────────────────────────────────────────────
        await CreateFavoritesAsync(dbContext, domainUsers, products);

        // ── Step 7: ProductComments ───────────────────────────────────────────
        await CreateProductCommentsAsync(dbContext, domainUsers, products);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Step helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static async Task<Dictionary<string, User>> CreateUsersAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        var domainUsers = new Dictionary<string, User>();

        foreach (var seed in RecommendationUsers)
        {
            // Identity layer
            var identityUser = await userManager.FindByEmailAsync(seed.Email);
            if (identityUser == null)
            {
                identityUser = new ApplicationUser
                {
                    UserName = seed.UserName,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                };

                var result = await userManager.CreateAsync(identityUser, DefaultPassword);
                if (!result.Succeeded)
                    throw new Exception(
                        $"Failed to create identity user {seed.Email}: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(identityUser, UserRole))
                await userManager.AddToRoleAsync(identityUser, UserRole);

            // Domain layer
            var domainUser = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

            if (domainUser == null)
            {
                domainUser = new User
                {
                    IdentityUserId = identityUser.Id,
                    Email = seed.Email,
                    FullName = seed.FullName,
                    Bio = seed.Bio,
                    City = seed.City,
                    Country = seed.Country,
                    IsActive = true,
                };

                dbContext.Add(domainUser);
                await dbContext.SaveChangesAsync();

                await userManager.AddClaimAsync(identityUser, new Claim(
                    "business_user_id",
                    domainUser.Id.ToString()));
            }

            domainUsers[seed.Email] = domainUser;
        }

        return domainUsers;
    }

    private static async Task<Dictionary<string, Product>> CreateProductsAsync(
        ApplicationDbContext dbContext,
        Dictionary<string, User> domainUsers)
    {
        var now = DateTime.UtcNow;
        var products = BuildProducts(now);

        // Load category slugs → IDs
        var categories = await dbContext.Categories
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Slug, c => c.Id);

        var createdProducts = new Dictionary<string, Product>();

        foreach (var seed in products)
        {
            if (!categories.TryGetValue(seed.CategorySlug, out var categoryId))
                throw new Exception($"Category slug '{seed.CategorySlug}' not found. " +
                                    "Ensure CategorySeeder has run before RecommendationSeeder.");

            if (!domainUsers.TryGetValue(seed.OwnerEmail, out var owner))
                throw new Exception($"Domain user '{seed.OwnerEmail}' not found.");

            var product = new RegularProduct
            {
                Title = seed.Title,
                Description = seed.Description,
                CategoryId = categoryId,
                OwnerUserId = owner.Id,
                Condition = seed.Condition,
                LocationCity = seed.City,
                LocationCountry = seed.Country,
                Status = ProductStatus.Active,
                Price = seed.Price,
                AllowNegotiation = seed.AllowNegotiation,
                CreatedAt = seed.CreatedAt,
                ProductImages = BuildImages(seed.Images, ProductImageType.Offer),
            };

            dbContext.Products.Add(product);
            createdProducts[seed.Title] = product;
        }

        await dbContext.SaveChangesAsync();
        return createdProducts;
    }

    private static async Task CreateFollowsAsync(
        ApplicationDbContext dbContext,
        Dictionary<string, User> users)
    {
        var userA = users[UserAEmail];
        var userB = users[UserBEmail];

        // User A follows User B (seller affinity)
        var alreadyFollows = await dbContext.Set<Follow>()
            .AnyAsync(f => f.FollowerId == userA.Id && f.FollowingId == userB.Id);

        if (!alreadyFollows)
        {
            dbContext.Add(new Follow
            {
                FollowerId = userA.Id,
                FollowingId = userB.Id,
                CreatedAt = DateTime.UtcNow,
            });

            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task CreateCategoryFollowsAsync(
        ApplicationDbContext dbContext,
        Dictionary<string, User> users)
    {
        var userA = users[UserAEmail];

        // Resolve the two parent-category IDs we need
        var targetSlugs = new[] { SlugElectronics, SlugBooks };

        var categories = await dbContext.Categories
            .Where(c => targetSlugs.Contains(c.Slug))
            .ToListAsync();

        foreach (var category in categories)
        {
            var exists = await dbContext.Set<CategoryFollow>()
                .AnyAsync(cf => cf.UserId == userA.Id && cf.CategoryId == category.Id);

            if (!exists)
            {
                dbContext.Add(new CategoryFollow
                {
                    UserId = userA.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task CreateFavoritesAsync(
        ApplicationDbContext dbContext,
        Dictionary<string, User> users,
        Dictionary<string, Product> products)
    {
        var userA = users[UserAEmail];

        // Resolve the sub-category IDs under Electronics and Books
        // to correctly identify products in those categories.
        var electronicsSlugs = new[] { SlugPhones, SlugLaptops, SlugAudio };
        var booksSlugs = new[] { SlugBooks };

        var electronicsCategoryIds = await dbContext.Categories
            .Where(c => electronicsSlugs.Contains(c.Slug))
            .Select(c => c.Id)
            .ToListAsync();

        var booksCategoryIds = await dbContext.Categories
            .Where(c => booksSlugs.Contains(c.Slug))
            .Select(c => c.Id)
            .ToListAsync();

        // Pick products NOT owned by User A
        var electronicsProducts = products.Values
            .Where(p => electronicsCategoryIds.Contains(p.CategoryId) && p.OwnerUserId != userA.Id)
            .Take(3)
            .ToList();

        var booksProducts = products.Values
            .Where(p => booksCategoryIds.Contains(p.CategoryId) && p.OwnerUserId != userA.Id)
            .Take(2)
            .ToList();

        var toFavorite = electronicsProducts.Concat(booksProducts);

        foreach (var product in toFavorite)
        {
            var exists = await dbContext.Set<Favorite>()
                .AnyAsync(f => f.UserId == userA.Id && f.ProductId == product.Id);

            if (!exists)
            {
                dbContext.Add(new Favorite
                {
                    UserId = userA.Id,
                    ProductId = product.Id,
                    CreatedAt = DateTime.UtcNow,
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task CreateProductCommentsAsync(
        ApplicationDbContext dbContext,
        Dictionary<string, User> users,
        Dictionary<string, Product> products)
    {
        // Commenters: B, C, D can comment on each other's and User A's products
        var commenterB = users[UserBEmail];
        var commenterC = users[UserCEmail];
        var commenterD = users[UserDEmail];

        // Products with 0 comments: leave them untouched (default)
        // Products with 3 comments: iPhone 15 Pro, Clean Code
        // Products with 10 comments: Samsung Galaxy S24 Ultra

        var threeCommentTargetTitles = new[]
        {
            "iPhone 15 Pro 256GB – Natural Titanium",
            "Clean Code by Robert C. Martin – Paperback",
        };

        var tenCommentTargetTitle = "Samsung Galaxy S24 Ultra 256GB – Titanium Black";

        // ── 3-comment products ───────────────────────────────────────────────

        foreach (var title in threeCommentTargetTitles)
        {
            if (!products.TryGetValue(title, out var product))
                continue;

            var existingCount = await dbContext.Set<ProductComment>()
                .CountAsync(c => c.ProductId == product.Id && !c.IsDeleted);

            if (existingCount > 0)
                continue;

            var threeComments = new[]
            {
                BuildComment(product.Id, commenterB.Id, "Is this still available?"),
                BuildComment(product.Id, commenterC.Id, "What's the lowest you'd accept?"),
                BuildComment(product.Id, commenterD.Id, "Looks great! Can it be shipped to Alexandria?"),
            };

            dbContext.AddRange(threeComments);
        }

        // ── 10-comment product ────────────────────────────────────────────────

        if (products.TryGetValue(tenCommentTargetTitle, out var hotProduct))
        {
            var existingCount = await dbContext.Set<ProductComment>()
                .CountAsync(c => c.ProductId == hotProduct.Id && !c.IsDeleted);

            if (existingCount == 0)
            {
                var commenters = new[] { commenterB, commenterC, commenterD };
                var bodies = new[]
                {
                    "Best Android flagship right now!",
                    "How's the camera compared to S23 Ultra?",
                    "Still under warranty?",
                    "Can I see the back panel?",
                    "Would you swap for an iPhone 14 Pro?",
                    "Is the S-Pen included?",
                    "Price is a bit high, any discount for quick sale?",
                    "Battery life after 3 months of use?",
                    "Does it have physical SIM or eSIM only?",
                    "I'll take it — how do we proceed?",
                };

                for (var i = 0; i < bodies.Length; i++)
                {
                    dbContext.Add(BuildComment(
                        hotProduct.Id,
                        commenters[i % commenters.Length].Id,
                        bodies[i]));
                }
            }
        }

        await dbContext.SaveChangesAsync();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private factory helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static List<ProductImage> BuildImages(SeedImage[] images, ProductImageType type)
    {
        var list = new List<ProductImage>();
        for (var i = 0; i < images.Length; i++)
        {
            list.Add(new ProductImage
            {
                Url = images[i].Url,
                DisplayOrder = i,
                Type = type,
                PublicId = $"rec_seed_{Guid.NewGuid():N}",
                CreatedAt = DateTime.UtcNow,
            });
        }
        return list;
    }

    private static ProductComment BuildComment(Guid productId, Guid userId, string body) =>
        new()
        {
            ProductId = productId,
            UserId = userId,
            Body = body,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
        };
}