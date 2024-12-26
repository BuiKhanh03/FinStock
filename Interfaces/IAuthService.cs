using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Models;

namespace api.Interfaces
{
    public interface IAuthService
    {
        public Task<string> LoginAsync(AppUser appUser, string pwd);
        public Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenModel model);
    }
}