using System.ComponentModel.DataAnnotations;

namespace AuctionService.Dtos;

public class SendOtpDto
{
    [Required]
    public string EmailOrPhone { get; set; } = string.Empty;
}
