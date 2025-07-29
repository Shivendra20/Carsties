namespace AuctionService.Entities;

[Table("Auctions")]
public class Auction
{
    public Guid Id { get; set; }
    public int ReservePrice { get; set; }
    public int CurrentPrice { get; set; }
    public string Seller { get; set; }
    public string Winner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public DateTime UpdateAt { get; set; }
    public Item Item { get; set; }
    public Status Status { get; set; }
}