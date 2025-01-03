using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Account;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace api.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<AppUser> _userManager;
        public AccountRepository(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<List<AppUser>> GetAllAsync()
        {
            return await _userManager.Users.ToListAsync();
        }
    }
}