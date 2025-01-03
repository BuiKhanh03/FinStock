using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace api.Dtos.Account
{
    public class AccountDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }

    }
}