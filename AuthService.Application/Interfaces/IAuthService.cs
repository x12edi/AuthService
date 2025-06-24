using AuthService.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterDto dto);
        Task<(string Token, string RefreshToken)?> LoginAsync(LoginDto dto);
        Task<(string Token, string RefreshToken)?> RefreshTokenAsync(RefreshTokenDto dto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);
        Task<IdentityResult> ConfirmEmailAsync(ConfirmEmailDto dto);
    }
}