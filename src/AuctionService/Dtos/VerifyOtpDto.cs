using System.ComponentModel.DataAnnotations;

namespace AuctionService.Dtos;

public class VerifyOtpDto
{
    [Required]
    public string EmailOrPhone { get; set; } = string.Empty;
    [Required]
    public string Otp { get; set; } = string.Empty;
}
