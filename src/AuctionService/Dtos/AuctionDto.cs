namespace AuctionService.Dtos;

public class AuctionsDto
{
    public Guid Id { get; set; }
    public int ReservePrice { get; set; }
    public int CurrentPrice { get; set; }
    public string Seller { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public int? SoldAmount { get; set; }
    public int? CurrentHighBit { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime EndedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdateAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
