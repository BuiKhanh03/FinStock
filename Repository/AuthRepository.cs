using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Interfaces;
using api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using api.Data;


namespace api.Services
{
    public class AuthRepository : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signinManager;
        private readonly ApplicationDBContext _context;

        public AuthRepository(UserManager<AppUser> userManager, IConfiguration config, ITokenService tokenService, SignInManager<AppUser> signInManager, ApplicationDBContext context)
        {
            _userManager = userManager;
            _config = config;
            _tokenService = tokenService;
            _signinManager = signInManager;
            _context = context;
        }

        public async Task<string> LoginAsync(AppUser appUser, string pwd)
        {
            var result = await _signinManager.CheckPasswordSignInAsync(appUser, pwd, false);
            if (!result.Succeeded) return null;
            var refreshToken = this.GenerateRefreshTokenString();
            appUser.RefreshToken = refreshToken;
            appUser.RefreshTokenExpiry = DateTime.Now.AddDays(1);
            await _userManager.UpdateAsync(appUser);
            return refreshToken;
        }
        public async Task<string> ChangePasswordUSerAsync(ChangePassword changePassword, ClaimsPrincipal user)
        {
            try
            {
                var appUser = await _userManager.FindByEmailAsync(user.FindFirst(ClaimTypes.Email)?.Value);
                if (appUser == null)
                {
                    return "User not found";
                }
                var result = await _userManager.ChangePasswordAsync(appUser, changePassword.CurrentPassword, changePassword.NewPassword);

                if (result.Succeeded)
                {
                    return "Password changed successfully.";
                }
            }
            catch (System.Exception)
            {

                throw;
            }

            return "Wrong password";
        }



        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenModel model)
        {
            var principal = this.GetTokenPrincipal(model.JwtToken);
            var response = new LoginResponseDto();
            if (principal == null || principal.Identity == null)
            {
                return response;
            }
            var givenName = principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
            if (givenName is null)
            {
                return response;
            }

            var identityUser = await _userManager.FindByNameAsync(givenName);

            if (identityUser is null || identityUser.RefreshToken != model.RefreshToken || identityUser.RefreshTokenExpiry < DateTime.Now)
                return response;

            response.IsLogedIn = true;
            response.JwtToken = _tokenService.CreateToken(identityUser);
            response.RefreshToken = this.GenerateRefreshTokenString();

            identityUser.RefreshToken = response.RefreshToken;
            identityUser.RefreshTokenExpiry = DateTime.Now.AddHours(12);
            await _userManager.UpdateAsync(identityUser);

            return response;
        }


        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];
            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<string> ConfirmEmailAsync(string email)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email.ToLower());
            if (user == null) return "Invalid email";
            user.EmailConfirmed = true;
            await _context.SaveChangesAsync();
            return "Email confirmed successfully";
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

        public async Task<string> NewPasswordAsync(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var newPassWord = GenerateRandomPassword();
            var result = await _userManager.ResetPasswordAsync(user, token, newPassWord);
            if (!result.Succeeded) return null;
            return newPassWord;
        }

        private string GenerateRandomPassword()

        {
            var random = new Random();
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
            var length = 12;
            var password = new string(Enumerable.Range(0, length)
                                                .Select(x => validChars[random.Next(validChars.Length)])
                                                .ToArray());
            return password;
        }


        private ClaimsPrincipal? GetTokenPrincipal(string token)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:SigningKey").Value));

            var validation = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidateLifetime = false,
                ValidateActor = false,
                ValidateIssuer = true,
                ValidIssuer = _config["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["JWT:Audience"],
            };
            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        }
    }
}