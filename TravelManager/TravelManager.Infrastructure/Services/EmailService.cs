using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TravelManager.Infrastructure.Interfaces;

namespace TravelManager.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var host = _configuration["EmailSettings:Host"];
                if (string.IsNullOrEmpty(host))
                {
                    host = _configuration["EmailSettings:SmtpServer"];
                }

                if (string.IsNullOrEmpty(host))
                {
                    throw new ArgumentNullException(nameof(host), "SMTP Host/Server не знайдено в appsettings.json. Додайте EmailSettings:Host або EmailSettings:SmtpServer");
                }

                var portString = _configuration["EmailSettings:Port"];
                int port = 587; 
                if (!string.IsNullOrEmpty(portString))
                {
                    int.TryParse(portString, out port);
                }

                var sslString = _configuration["EmailSettings:EnableSSL"];
                if (string.IsNullOrEmpty(sslString))
                {
                    sslString = _configuration["EmailSettings:EnableSsl"];
                }

                bool enableSSL = true;
                if (!string.IsNullOrEmpty(sslString))
                {
                    bool.TryParse(sslString, out enableSSL);
                }

                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                if (string.IsNullOrEmpty(senderEmail))
                {
                    throw new ArgumentNullException(nameof(senderEmail), "Email відправника не знайдено в appsettings.json. Додайте EmailSettings:SenderEmail");
                }

                var senderName = _configuration["EmailSettings:SenderName"] ?? "TravelManager Admin";

                var password = _configuration["EmailSettings:Password"];
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException(nameof(password), "Пароль не знайдено в appsettings.json. Додайте EmailSettings:Password");
                }

                using (var client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(senderEmail, password);
                    client.EnableSsl = enableSSL;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Помилка при відправці листа на {email}: {ex.Message}", ex);
            }
        }
    }
}