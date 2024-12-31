using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Interfaces
{
    public interface IAuthService
    {
        public Task<string> LoginAsync(AppUser appUser, string pwd);
        public Task<string> ChangePasswordUSerAsync(ChangePassword changePassword, ClaimsPrincipal user);

        public Task<string> ConfirmEmailAsync(string email);
        public Task<string> ForgotPasswordAsync(string email);
        public Task<string> NewPasswordAsync(string email, string token);

        public Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenModel model);
    }
}