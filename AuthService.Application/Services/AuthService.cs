using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return result;
            }
            await _userManager.AddToRoleAsync(user, "User");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{_configuration["App:BaseUrl"]}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
            await SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.");
            await _unitOfWork.CompleteAsync();
            return result;
        }
        public async Task<(string Token, string RefreshToken)?> LoginAsync(LoginDto dto)
        {
            var user = await _unitOfWork.Users.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return null;
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return null;
            }
            var token = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            return (token, refreshToken);
        }
        public async Task<(string Token, string RefreshToken)?> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var user = await _unitOfWork.Users.FindAsync(u => u.RefreshToken == dto.RefreshToken);
            var singleUser = user.FirstOrDefault();
            if (singleUser == null || singleUser.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return null;
            }
            var token = GenerateJwtToken(singleUser);
            var newRefreshToken = Guid.NewGuid().ToString();
            singleUser.RefreshToken = newRefreshToken;
            singleUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _unitOfWork.Users.Update(singleUser);
            await _unitOfWork.CompleteAsync();
            return (token, newRefreshToken);
        }
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _unitOfWork.Users.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return false;
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_configuration["App:BaseUrl"]}/api/auth/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";
            await SendEmailAsync(user.Email, "Reset your password", $"Reset your password by clicking <a href='{resetLink}'>here</a>.");
            return true;
        }
        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Invalid user." });
            }
            return await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        }
        public async Task<IdentityResult> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Invalid user." });
            }
            return await _userManager.ConfirmEmailAsync(user, dto.Token);
        }
        private string GenerateJwtToken(ApplicationUser user)
        {
            // Fetch roles assigned to the user
            var roles = _userManager.GetRolesAsync(user).Result;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
            };

            // Add each role as a separate claim
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            //email.From.Add(new MailboxAddress("Auth Service", _configuration["Email:SmtpUsername"]));
            email.From.Add(new MailboxAddress("Auth Service", "notification@mptechnologies.com"));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };
            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["Email:SmtpUsername"], _configuration["Email:SmtpPassword"]);
            await client.SendAsync(email);
            await client.DisconnectAsync(true);
        }
    }
}


