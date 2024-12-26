using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Account
{
    public class LoginResponseDto
    {
        public bool IsLogedIn { get; set; } = false;
        public string JwtToken { get; set; }
        public string RefreshToken { get; internal set; }
    }
}