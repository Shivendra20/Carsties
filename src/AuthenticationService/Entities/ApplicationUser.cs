using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Entities;

public class ApplicationUser : IdentityUser
{
    public string? Otp { get; set; }
    public DateTime? OtpExpiration { get; set; }
}
