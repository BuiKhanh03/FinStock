using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using api.Interfaces;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Org.BouncyCastle.Asn1.Tsp;
using Microsoft.AspNetCore.Identity;
using api.Models;
using Microsoft.EntityFrameworkCore;
using api.Data;

namespace api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDBContext _context;
        public EmailService(IConfiguration config, UserManager<AppUser> userManager, ApplicationDBContext context)
        {
            _config = config;
            _userManager = userManager;
            _context = context;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlBody)
        {
            try
            {
                var emailToSend = new MimeMessage();
                emailToSend.From.Add(new MailboxAddress("FinStock", _config["Email:EmailSender"]));
                emailToSend.To.Add(MailboxAddress.Parse(email));
                emailToSend.Subject = subject;

                emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlBody
                };

                // Validate the SMTP port
                if (!int.TryParse(_config["Email:SmtpPort"], out int smtpPort))
                {
                    throw new ArgumentException("Invalid SMTP port configuration.");
                }

                using (var emailClient = new SmtpClient())
                {

                    // Connect and authenticate asynchronously
                    await emailClient.ConnectAsync(_config["Email:SmtpServer"], smtpPort, SecureSocketOptions.StartTls);
                    await emailClient.AuthenticateAsync(_config["Email:EmailSender"], _config["Email:EmailPassword"]);

                    // Send the email
                    await emailClient.SendAsync(emailToSend);

                    // Disconnect
                    await emailClient.DisconnectAsync(true);
                }
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid email format.");
            }
            catch (Exception ex)
            {
                // Log or handle the exception accordingly
                throw new Exception("An error occurred while sending the email.", ex);
            }
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

        public async Task<string> NewPassword(string email, string token)
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
    }
}