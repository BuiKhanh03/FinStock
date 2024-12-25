using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using api.Interfaces;
using Microsoft.EntityFrameworkCore;
using api.Services;
using System.Runtime.CompilerServices;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        public readonly IEmailService _emailService;
        private readonly SignInManager<AppUser> _signinManager;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IEmailService emailService
, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _signinManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
            if (user == null) return Unauthorized("Invalid username");
            // Check if password matches
            var result = await _signinManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized("Username not found or password incorrect");
            // Check if the email is confirmed
            if (!user.EmailConfirmed) return Unauthorized("Please confirm your email before logging in.");
            return Ok(new NewUserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = _tokenService.CreateToken(user)
            });
        }

        [HttpGet("confirm-email")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ConfirmEmail([FromBody] string email)
        {

            var result = await _emailService.ConfirmEmailAsync(email);
            if (result == "Invalid email") return BadRequest("Cannot confirm your email");
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var appUser = new AppUser
                {
                    UserName = registerDto.UserName,
                    Email = registerDto.Email,
                    IsActive = false
                };
                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);
                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "USER");

                    if (roleResult.Succeeded)
                    {

                        // Tạo liên kết xác nhận email
                        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { email = appUser.Email }, Request.Scheme);
                        var emailContent = $"Please confirm your account by clicking <a href=\"{confirmationLink}\">here</a>";
                        // Gửi email
                        await _emailService.SendEmailAsync(appUser.Email, "Confirm your email", emailContent);
                        return Ok("Registration successful. Please check your email to confirm your account.");
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return StatusCode(500, createdUser.Errors);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("/forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] PasswordDto passwordDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var tokenReset = await _emailService.ForgotPasswordAsync(passwordDto.Email);
                if (tokenReset == null) return BadRequest("Invalid Email");
                // Tạo liên kết xác nhận email và token
                var confirmationLink = Url.Action("NewPassWord", "Account", new { email = passwordDto.Email, token = tokenReset }, Request.Scheme);
                var emailContent = $"Please confirm your account by clicking <a href=\"{confirmationLink}\">here</a>";
                await _emailService.SendEmailAsync(passwordDto.Email, "Create new password", emailContent);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
            return Ok("Please check your email to create a new password");
        }

        [HttpGet("new-password")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> NewPassword(string email, string token)
        {
            var result = await _emailService.NewPassword(email, token);
            if (result == null) return BadRequest("Failed to create new password");
            await _emailService.SendEmailAsync(email, "Your new password", $"Your password has been reset. Your new password is: {result}");
            return Ok("Password has been reset successfully.");
        }
    }
}