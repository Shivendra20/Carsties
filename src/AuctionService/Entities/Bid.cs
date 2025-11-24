namespace AuctionService.Entities;

public class Bid
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string Bidder { get; set; } = string.Empty;
    public int Amount { get; set; }
    public DateTime BidTime { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Auction Auction { get; set; } = null!;
}
