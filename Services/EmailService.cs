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

        public EmailService(IConfiguration config)
        {
            _config = config;
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

      
    }
}