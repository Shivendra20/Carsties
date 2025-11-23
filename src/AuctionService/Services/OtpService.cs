using AuctionService.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuctionService.Services;

public class OtpService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OtpService> _logger;

    public OtpService(UserManager<ApplicationUser> userManager, ILogger<OtpService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public string GenerateOtp()
    {
        // Generate a 6-digit OTP
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public async Task<bool> SendOtpAsync(ApplicationUser user, string otp)
    {
        // Store OTP in user record (expires in 10 minutes)
        user.Otp = otp;
        user.OtpExpiration = DateTime.UtcNow.AddMinutes(10);
        
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            // TODO: In production, send OTP via Email or SMS service
            _logger.LogInformation($"OTP generated for user {user.Email}: {otp}");
            
            // For development, log the OTP
            Console.WriteLine($"=== OTP for {user.Email} or {user.PhoneNumber}: {otp} ===");
            
            return true;
        }
        
        return false;
    }

    public async Task<bool> VerifyOtpAsync(ApplicationUser user, string otp)
    {
        if (user.Otp == null || user.OtpExpiration == null)
        {
            return false;
        }

        if (user.OtpExpiration < DateTime.UtcNow)
        {
            // OTP expired
            return false;
        }

        if (user.Otp != otp)
        {
            return false;
        }

        // Clear OTP after successful verification
        user.Otp = null;
        user.OtpExpiration = null;
        await _userManager.UpdateAsync(user);

        return true;
    }
}
