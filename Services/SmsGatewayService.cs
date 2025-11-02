using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class SmsGatewayService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;

    public SmsGatewayService(IConfiguration config)
    {
        _smtpHost = config["Smtp:Host"];
        _smtpPort = int.Parse(config["Smtp:Port"]);
        _smtpUser = config["Smtp:User"];
        _smtpPass = config["Smtp:Pass"];
        _fromEmail = config["Smtp:FromEmail"];
    }

    public async Task SendSmsAsync(string phoneNumber, string carrierDomain, string message)
    {
        var toAddress = $"{phoneNumber}@{carrierDomain}";

        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUser, _smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(_fromEmail, toAddress)
        {
            Subject = "",
            Body = message
        };

        await client.SendMailAsync(mailMessage);
    }
}
