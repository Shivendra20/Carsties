using System.ComponentModel.DataAnnotations;

namespace AuctionService.Dtos;

public class CreateBidDto
{
    [Required]
    public Guid AuctionId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
    public int Amount { get; set; }
}
