namespace AuthenticationService.Dtos;

public class ResetPasswordDto
{
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
