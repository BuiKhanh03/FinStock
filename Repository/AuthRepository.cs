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


namespace api.Services
{
    public class AuthRepository : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signinManager;

        public AuthRepository(UserManager<AppUser> userManager, IConfiguration config, ITokenService tokenService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _config = config;
            _tokenService = tokenService;
            _signinManager = signInManager;
        }

        public async Task<string> LoginAsync(AppUser appUser, string pwd)
        {
            // Check if password matches
            var result = await _signinManager.CheckPasswordSignInAsync(appUser, pwd, false);
            if (!result.Succeeded) return "username";
            // Check if the email is confirmed
            if (!appUser.EmailConfirmed) return "email";
            var refreshToken = this.GenerateRefreshTokenString();
            return refreshToken;
        }


        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenModel model)
        {
            var principal = GetTokenPrincipal(model.JwtToken);
            Console.WriteLine(principal.Identity.Name);

            var response = new LoginResponseDto();
            if (principal?.FindFirst("given_name")?.Value is null)
            {
                return response;
            }

            var identityUser = await _userManager.FindByNameAsync(principal?.FindFirst("given_name").Value);

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

        private ClaimsPrincipal? GetTokenPrincipal(string token)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:SigningKey").Value));

            var validation = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidateLifetime = false,
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateAudience = false,
            };
            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        }
    }
}