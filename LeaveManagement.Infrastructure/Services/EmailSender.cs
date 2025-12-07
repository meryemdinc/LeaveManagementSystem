using LeaveManagement.Application.Contracts.Infrastructure;
using LeaveManagement.Application.DTOs.Email;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LeaveManagement.Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmail(EmailRequest emailRequest)
        {
            try
            {
                // Ayarları appsettings.json'dan oku
                var host = _configuration["EmailSettings:Host"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var fromEmail = _configuration["EmailSettings:From"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];

                // SMTP İstemcisini Hazırla
                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true; // Güvenli bağlantı
                    client.Credentials = new NetworkCredential(username, password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = emailRequest.Subject,
                        Body = emailRequest.Body,
                        IsBodyHtml = true // HTML formatında mail atabiliriz
                    };

                    mailMessage.To.Add(emailRequest.To);

                    // Gönder!
                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Hata olursa loglayabiliriz ama şimdilik false dönelim
                // Console.WriteLine(ex.Message); 
                return false;
            }
        }
    }
}