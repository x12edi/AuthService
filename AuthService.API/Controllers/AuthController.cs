using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = result.Errors.FirstOrDefault()?.Description ?? "Registration failed" });
            }
            return Ok("User registered successfully. Please check your email to confirm.");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                return Unauthorized("Invalid credentials or email not confirmed.");
            }
            return Ok(new { Token = result.Value.Token, RefreshToken = result.Value.RefreshToken });
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto);
            if (result == null)
            {
                return Unauthorized("Invalid refresh token.");
            }
            return Ok(new { Token = result.Value.Token, RefreshToken = result.Value.RefreshToken });
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto);
            if (!result)
            {
                return BadRequest(new { Message = "Invalid email" });
            }
            return Ok("Password reset link sent.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = result.Errors.FirstOrDefault()?.Description ?? "Password reset failed" });
            }
            return Ok("Password reset successfully.");
        }

        //[HttpPost("reset-password")]
        //public async Task<IActionResult> ResetPassword([FromQuery] string userId, [FromQuery] string token, [FromBody] ResetPasswordRequestDto dto)
        //{
        //    var resetDto = new ResetPasswordDto
        //    {
        //        UserId = userId,
        //        Token = token,
        //        NewPassword = dto.NewPassword
        //    };
        //    var result = await _authService.ResetPasswordAsync(resetDto);
        //    if (!result.Succeeded)
        //    {
        //        return BadRequest(result.Errors);
        //    }
        //    return Ok("Password reset successfully.");
        //}

        //[HttpPost("confirm-email")]
        //public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        //{
        //    var result = await _authService.ConfirmEmailAsync(dto);
        //    if (!result.Succeeded)
        //    {
        //        return BadRequest(result.Errors);
        //    }
        //    return Ok("Email confirmed successfully.");
        //}

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(new ConfirmEmailDto { UserId = userId, Token = token });
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = result.Errors.FirstOrDefault()?.Description ?? "Email confirmation failed" });
            }
            return Ok(new { Message = "Email confirmed successfully" });
        }
    }
}