using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Models;

namespace api.Mappers
{
    public static class AccountMapper
    {
        public static AccountDto ToAccountDto(this AppUser user)
        {
            return new AccountDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
        }

    }
}