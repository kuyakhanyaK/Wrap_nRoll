using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Wrap_nRoll.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _fromEmail;

        public EmailService(IConfiguration config)
        {
            _host = config["EmailSettings:SmtpServer"];
            _port = int.Parse(config["EmailSettings:Port"]);
            _user = config["EmailSettings:Username"];
            _pass = config["EmailSettings:Password"];
            _fromEmail = config["EmailSettings:SenderEmail"];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_user, _pass)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Wrap 'n Roll"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(to);
            await client.SendMailAsync(mailMessage);
        }
    }
}
