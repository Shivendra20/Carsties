using AuthenticationService.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Services;

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
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public async Task<bool> SendOtpAsync(ApplicationUser user, string otp)
    {
        user.Otp = otp;
        user.OtpExpiration = DateTime.UtcNow.AddMinutes(10);

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation($"OTP generated for user {user.Email}: {otp}");
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
            return false;
        }

        if (user.Otp != otp)
        {
            return false;
        }

        user.Otp = null;
        user.OtpExpiration = null;
        await _userManager.UpdateAsync(user);

        return true;
    }
}
