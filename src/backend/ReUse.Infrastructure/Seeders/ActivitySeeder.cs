using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

using ReUse.Domain.Entities;

using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Seeders;

public static class ActivitySeeder
{
    private static readonly Dictionary<string, string> ProductKeywords = new()
    {
        ["iPhone 13 Pro"] = "iPhone 13 Pro",
        ["Sony WH-1000XM4"] = "Sony WH-1000XM4",
        ["MacBook Pro 14"] = "MacBook Pro 14",
        ["Gaming Chair"] = "Gaming Chair",
        ["Mechanical Keyboard"] = "Mechanical Keyboard",
        ["Vintage Leather Jacket"] = "Vintage Leather Jacket",
        ["PS5 Digital Edition"] = "PlayStation 5",
        ["Gaming Monitor"] = "Gaming Monitor",
        ["Designer Handbag"] = "Designer Handbag",
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var config = services.GetRequiredService<IConfiguration>();

        // Clear existing to re-seed with ProductId links
        var existing = await dbContext.Set<ActivityEvent>().ToListAsync();
        if (existing.Count > 0)
        {
            dbContext.Set<ActivityEvent>().RemoveRange(existing);
            await dbContext.SaveChangesAsync();
        }

        var users = await dbContext.Set<User>().ToDictionaryAsync(u => u.Email, u => u.Id);
        var products = await dbContext.Set<Product>().ToListAsync();

        var adminEmail = config["ADMIN:EMAIL"];
        var now = DateTime.UtcNow;

        var activities = new List<ActivityEvent>();

        Guid? ResolveProductId(string keyword)
        {
            if (!ProductKeywords.TryGetValue(keyword, out var search))
                return null;
            return products.FirstOrDefault(p =>
                p.Title.Contains(search, StringComparison.OrdinalIgnoreCase))?.Id;
        }

        // Admin activities
        if (adminEmail != null && users.TryGetValue(adminEmail, out var adminId))
        {
            activities.AddRange(new[]
            {
                new ActivityEvent
                {
                    UserId = adminId, Type = "product.created", Description = "Created product: iPhone 13 Pro",
                    ProductId = ResolveProductId("iPhone 13 Pro"),
                    Timestamp = now.AddDays(-30), CreatedAt = now.AddDays(-30)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "product.viewed", Description = "Viewed product: Sony WH-1000XM4",
                    ProductId = ResolveProductId("Sony WH-1000XM4"),
                    Timestamp = now.AddDays(-29), CreatedAt = now.AddDays(-29)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "searched", Description = "searched for \"wireless headphones\"",
                    Timestamp = now.AddDays(-28), CreatedAt = now.AddDays(-28)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "product.created", Description = "Created product: MacBook Pro 14",
                    ProductId = ResolveProductId("MacBook Pro 14"),
                    Timestamp = now.AddDays(-27), CreatedAt = now.AddDays(-27)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "product.viewed", Description = "Viewed product: Canon EOS R6",
                    ProductId = ResolveProductId("Canon EOS R6"),
                    Timestamp = now.AddDays(-25), CreatedAt = now.AddDays(-25)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "product.updated", Description = "Updated product: iPhone 13 Pro",
                    ProductId = ResolveProductId("iPhone 13 Pro"),
                    Timestamp = now.AddDays(-24), CreatedAt = now.AddDays(-24)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "favorite.added", Description = "Added 'Sony WH-1000XM4' to favorites",
                    ProductId = ResolveProductId("Sony WH-1000XM4"),
                    Timestamp = now.AddDays(-20), CreatedAt = now.AddDays(-20)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "user.followed", Description = "Followed user: Ahmed Mohamed",
                    Timestamp = now.AddDays(-15), CreatedAt = now.AddDays(-15)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "searched", Description = "searched for \"vintage camera\"",
                    Timestamp = now.AddDays(-12), CreatedAt = now.AddDays(-12)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "favorite.added", Description = "Added 'Canon EOS R6' to favorites",
                    ProductId = ResolveProductId("Canon EOS R6"),
                    Timestamp = now.AddDays(-10), CreatedAt = now.AddDays(-10)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "product.viewed", Description = "Viewed product: Gaming Chair",
                    ProductId = ResolveProductId("Gaming Chair"),
                    Timestamp = now.AddDays(-8), CreatedAt = now.AddDays(-8)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "product.deleted", Description = "Deleted product: Old Listing",
                    Timestamp = now.AddDays(-7), CreatedAt = now.AddDays(-7)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "user.followed", Description = "Followed user: Farah Hazem",
                    Timestamp = now.AddDays(-5), CreatedAt = now.AddDays(-5)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "product.updated", Description = "Updated product: MacBook Pro 14",
                    ProductId = ResolveProductId("MacBook Pro 14"),
                    Timestamp = now.AddDays(-3), CreatedAt = now.AddDays(-3)
                },
                new ActivityEvent
                {
                    UserId = adminId, Type = "searched", Description = "searched for \"macbook accessories\"",
                    Timestamp = now.AddDays(-2), CreatedAt = now.AddDays(-2)
                },
            });
        }

        // Seed user: Ahmed Mordi
        if (users.TryGetValue("ahmed.mordi@reuse.dev", out var ahmedMordiId))
        {
            activities.AddRange(new[]
            {
                new ActivityEvent
                {
                    UserId = ahmedMordiId, Type = "product.created", Description = "Created product: iPhone 13 Pro",
                    ProductId = ResolveProductId("iPhone 13 Pro"),
                    Timestamp = now.AddDays(-29), CreatedAt = now.AddDays(-29)
                },
                new ActivityEvent
                {
                    UserId = ahmedMordiId, Type = "product.created", Description = "Created product: MacBook Pro 14",
                    ProductId = ResolveProductId("MacBook Pro 14"),
                    Timestamp = now.AddDays(-27), CreatedAt = now.AddDays(-27)
                },
                new ActivityEvent
                {
                    UserId = ahmedMordiId, Type = "product.created", Description = "Created product: Sony WH-1000XM4",
                    ProductId = ResolveProductId("Sony WH-1000XM4"),
                    Timestamp = now.AddDays(-26), CreatedAt = now.AddDays(-26)
                },
                new ActivityEvent
                {
                    UserId = ahmedMordiId, Type = "favorite.added", Description = "Added 'Mechanical Keyboard' to favorites",
                    ProductId = ResolveProductId("Mechanical Keyboard"),
                    Timestamp = now.AddDays(-18), CreatedAt = now.AddDays(-18)
                },
                new ActivityEvent
                {
                    UserId = ahmedMordiId, Type = "product.viewed", Description = "Viewed product: MacBook Pro 14",
                    ProductId = ResolveProductId("MacBook Pro 14"),
                    Timestamp = now.AddDays(-14), CreatedAt = now.AddDays(-14)
                },
                new ActivityEvent
                {
                    UserId = ahmedMordiId, Type = "user.followed", Description = "Followed user: Omar Goher",
                    Timestamp = now.AddDays(-12), CreatedAt = now.AddDays(-12)
                },
                new ActivityEvent
                {
                    UserId = ahmedMordiId, Type = "product.viewed", Description = "Viewed product: Sony WH-1000XM4",
                    ProductId = ResolveProductId("Sony WH-1000XM4"),
                    Timestamp = now.AddDays(-9), CreatedAt = now.AddDays(-9)
                },
            });
        }

        // Seed user: Farah Hazem
        if (users.TryGetValue("farah.hazem@reuse.dev", out var farahId))
        {
            activities.AddRange(new[]
            {
                new ActivityEvent
                {
                    UserId = farahId, Type = "product.created", Description = "Created product: Vintage Leather Jacket",
                    ProductId = ResolveProductId("Vintage Leather Jacket"),
                    Timestamp = now.AddDays(-22), CreatedAt = now.AddDays(-22)
                },
                new ActivityEvent
                {
                    UserId = farahId, Type = "product.viewed", Description = "Viewed product: Gaming Chair",
                    ProductId = ResolveProductId("Gaming Chair"),
                    Timestamp = now.AddDays(-16), CreatedAt = now.AddDays(-16)
                },
                new ActivityEvent
                {
                    UserId = farahId, Type = "favorite.added", Description = "Added 'Designer Handbag' to favorites",
                    ProductId = ResolveProductId("Designer Handbag"),
                    Timestamp = now.AddDays(-14), CreatedAt = now.AddDays(-14)
                },
                new ActivityEvent
                {
                    UserId = farahId, Type = "user.followed", Description = "Followed user: Tasabeih Talaat",
                    Timestamp = now.AddDays(-8), CreatedAt = now.AddDays(-8)
                },
            });
        }

        // Seed user: Omar Goher
        if (users.TryGetValue("omar.goher@reuse.dev", out var omarId))
        {
            activities.AddRange(new[]
            {
                new ActivityEvent
                {
                    UserId = omarId, Type = "product.created", Description = "Created product: PS5 Digital Edition",
                    ProductId = ResolveProductId("PS5 Digital Edition"),
                    Timestamp = now.AddDays(-19), CreatedAt = now.AddDays(-19)
                },
                new ActivityEvent
                {
                    UserId = omarId, Type = "favorite.added", Description = "Added 'Gaming Monitor 27\"' to favorites",
                    ProductId = ResolveProductId("Gaming Monitor"),
                    Timestamp = now.AddDays(-11), CreatedAt = now.AddDays(-11)
                },
                new ActivityEvent
                {
                    UserId = omarId, Type = "product.updated", Description = "Updated product: PS5 Digital Edition",
                    ProductId = ResolveProductId("PS5 Digital Edition"),
                    Timestamp = now.AddDays(-6), CreatedAt = now.AddDays(-6)
                },
                new ActivityEvent
                {
                    UserId = omarId, Type = "product.viewed", Description = "Viewed product: PS5 Digital Edition",
                    ProductId = ResolveProductId("PS5 Digital Edition"),
                    Timestamp = now.AddDays(-4), CreatedAt = now.AddDays(-4)
                },
            });
        }

        dbContext.Set<ActivityEvent>().AddRange(activities);
        await dbContext.SaveChangesAsync();
    }
}