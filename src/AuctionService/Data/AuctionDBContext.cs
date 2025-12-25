using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDBContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser>
{
    public AuctionDBContext(DbContextOptions<AuctionDBContext> options) : base(options)
    {
    }

    public DbSet<Auction> Auctions { get; set; } 
    public DbSet<Item> Items { get; set; } 
    public DbSet<Bid> Bids { get; set; } 

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
            
        modelBuilder.Entity<Bid>()
            .HasOne(b => b.Auction)
            .WithMany()
            .HasForeignKey(b => b.AuctionId);
    }
}