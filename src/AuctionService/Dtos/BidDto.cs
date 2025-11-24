namespace AuctionService.Dtos;

public class BidDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string Bidder { get; set; } = string.Empty;
    public int Amount { get; set; }
    public DateTime BidTime { get; set; }
}
