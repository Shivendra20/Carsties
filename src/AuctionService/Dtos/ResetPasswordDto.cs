using System.ComponentModel.DataAnnotations;

namespace AuctionService.Dtos;

public class ResetPasswordDto
{
    [Required]
    public string EmailOrPhone { get; set; } = string.Empty;
    [Required]
    public string Otp { get; set; } = string.Empty;
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
