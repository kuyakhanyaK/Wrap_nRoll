using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Wrap_nRoll.Services
{
    public class EmailToSmsService : ISmsService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _fromEmail;

        public EmailToSmsService(IConfiguration config)
        {
            _host = config["Smtp:Host"];
            _port = int.Parse(config["Smtp:Port"]);
            _user = config["Smtp:User"];
            _pass = config["Smtp:Pass"];
            _fromEmail = config["Smtp:FromEmail"];
        }

        public async Task SendSmsAsync(string to, string carrierGateway, string message)
        {
            // If no carrier gateway provided, do nothing (avoid errors)
            if (string.IsNullOrWhiteSpace(carrierGateway))
                return;

            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_user, _pass)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = string.Empty, // SMS doesn’t need a subject
                Body = message
            };

            // Example: 27831234567@mtn.co.za
            mailMessage.To.Add($"{to}@{carrierGateway}");

            await client.SendMailAsync(mailMessage);
        }
    }
}
