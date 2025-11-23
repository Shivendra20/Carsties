using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class DbInitializer
{
    public static void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        // Initialize roles
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        InitializeRoles(roleManager).Wait();
        
        SeedData(scope.ServiceProvider.GetService<AuctionDBContext>());
    }
    
    private static async Task InitializeRoles(Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager)
    {
        string[] roleNames = { "Bidder", "Auctioneer" };
        
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(roleName));
            }
        }
    }

    public static void SeedData(AuctionDBContext context)
    {
        context.Database.Migrate();

        if (context.Auctions.Any())
        {
            return;
        }

        /*
        var auctions = new List<Entities.Auction>
        {
            // 1 Ford GT
            new Auction
            {
                Id = Guid.Parse("afbee524-5972-4075-8800-7d1f9d7b0a0c"),
                Status = Status.Live,
                ReservePrice = 20000,
                Seller = "bob",
                EndedAt = DateTime.UtcNow.AddDays(10),
                Item = new Item
                {
                    Make = "Ford",
                    Model = "GT",
                    Color = "White",
                    Mileage = 50000,
                    Year = 2020,
                    ImageUrl = "https://cdn.pixabay.com/photo/2016/05/06/16/32/car-1376190_960_720.jpg"
                }
            },
            // ... (other items omitted for brevity)
        };

        context.Auctions.AddRange(auctions);

        context.SaveChanges();
        */
    }
}