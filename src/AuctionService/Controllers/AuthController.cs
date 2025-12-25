using AuctionService.Dtos;
using AuctionService.Entities;
using AuctionService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TokenService _tokenService;
    private readonly OtpService _otpService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TokenService tokenService,
        OtpService otpService
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _otpService = otpService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user =
            await _userManager.FindByEmailAsync(loginDto.Username)
            ?? await _userManager.FindByNameAsync(loginDto.Username);

        if (user == null)
            return Unauthorized("Invalid credentials");

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

        if (result)
        {
            return await CreateUserObject(user);
        }

        return Unauthorized("Invalid credentials");
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (!new[] { "Bidder", "Auctioneer", "Both" }.Contains(registerDto.Role))
        {
            return BadRequest("Invalid role. Must be 'Bidder', 'Auctioneer', or 'Both'");
        }

        if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
        {
            return BadRequest("Email already taken");
        }

        if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
        {
            return BadRequest("Username already taken");
        }

        if (await _userManager.Users.AnyAsync(x => x.PhoneNumber == registerDto.PhoneNumber))
        {
            return BadRequest("Phone number already registered");
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            if (registerDto.Role == "Both")
            {
                await _userManager.AddToRoleAsync(user, "Bidder");
                await _userManager.AddToRoleAsync(user, "Auctioneer");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, registerDto.Role);
            }

            return await CreateUserObject(user);
        }

        return BadRequest(result.Errors);
    }

    [AllowAnonymous]
    [HttpPost("send-otp")]
    public async Task<ActionResult> SendOtp(SendOtpDto sendOtpDto)
    {
        var user = await FindUserByEmailOrPhone(sendOtpDto.EmailOrPhone);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var otp = _otpService.GenerateOtp();
        var result = await _otpService.SendOtpAsync(user, otp);

        if (result)
        {
            return Ok(new { message = "OTP sent successfully" });
        }

        return BadRequest("Failed to send OTP");
    }

    [AllowAnonymous]
    [HttpPost("login-with-otp")]
    public async Task<ActionResult<UserDto>> LoginWithOtp(VerifyOtpDto verifyOtpDto)
    {
        var user = await FindUserByEmailOrPhone(verifyOtpDto.EmailOrPhone);

        if (user == null)
        {
            return Unauthorized("Invalid credentials");
        }

        var isValid = await _otpService.VerifyOtpAsync(user, verifyOtpDto.Otp);

        if (isValid)
        {
            return await CreateUserObject(user);
        }

        return Unauthorized("Invalid or expired OTP");
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(SendOtpDto sendOtpDto)
    {
        var user = await FindUserByEmailOrPhone(sendOtpDto.EmailOrPhone);
        if (user == null)
        {
            return Ok(new { message = "If the account exists, an OTP has been sent" });
        }
        var otp = _otpService.GenerateOtp();
        var result = await _otpService.SendOtpAsync(user, otp);

        return Ok(new { message = "If the account exists, an OTP has been sent" });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user = await FindUserByEmailOrPhone(resetPasswordDto.EmailOrPhone);

        if (user == null)
        {
            return BadRequest("Invalid request");
        }

        var isValid = await _otpService.VerifyOtpAsync(user, resetPasswordDto.Otp);

        if (!isValid)
        {
            return BadRequest("Invalid or expired OTP");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(
            user,
            token,
            resetPasswordDto.NewPassword
        );

        if (result.Succeeded)
        {
            return Ok(new { message = "Password reset successfully" });
        }

        return BadRequest(result.Errors);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await _userManager.FindByEmailAsync(
            User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        );

        if (user == null)
        {
            return Unauthorized();
        }

        return await CreateUserObject(user);
    }

    private async Task<UserDto> CreateUserObject(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            DisplayName = user.UserName,
            Token = await _tokenService.CreateToken(user, _userManager),
            Username = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList(),
        };
    }

    private async Task<ApplicationUser?> FindUserByEmailOrPhone(string emailOrPhone)
    {
        return await _userManager.FindByEmailAsync(emailOrPhone)
            ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == emailOrPhone);
    }
}
