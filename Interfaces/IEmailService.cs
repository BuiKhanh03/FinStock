using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlBody);
        Task<string> ConfirmEmailAsync(string email);
        Task<string> ForgotPasswordAsync(string email);
        Task<string> NewPassword(string email, string token);
    }
}