using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDBContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<Microsoft.AspNetCore.Identity.IdentityUser>
{
    public AuctionDBContext(DbContextOptions<AuctionDBContext> options) : base(options)
    {
    }

    public DbSet<Auction> Auctions { get; set; } // Auctions table
    public DbSet<Item> Items { get; set; } // Items table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Auction>()
            .HasOne(a => a.Item)
            .WithOne(i => i.Auction)
            .HasForeignKey<Item>(i => i.AuctionId);

        modelBuilder.Entity<Item>()
            .Property(i => i.ImageUrl)
            .IsRequired(false);
    }
}