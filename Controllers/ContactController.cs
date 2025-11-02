using Microsoft.AspNetCore.Mvc;
using Wrap_nRoll.Models;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Wrap_nRoll.Controllers
{
    public class ContactController : Controller
    {
        private readonly IConfiguration _config;

        public ContactController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactFormModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // ✅ Match appsettings.json keys
                var senderEmail = _config["EmailSettings:Username"];
                var senderPassword = _config["EmailSettings:Password"];
                var receiverEmail = _config["EmailSettings:SenderEmail"];

                // 📨 Business notification email
                string body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #fdfaf6; border-radius: 10px; border: 1px solid #f1e1b0;'>
                        <h2 style='color: #dba400; text-align: center;'>🍔 Wrap 'n Roll - New Contact Message</h2>
                        <hr style='border: 1px solid #ffd76b;' />
                        <p><strong>From:</strong> {model.Name} ({model.Email})</p>
                        <p><strong>Subject:</strong> {model.Subject}</p>
                        <div style='margin-top: 15px;'>
                            <p><strong>Message:</strong></p>
                            <p style='background-color: #fff8e1; padding: 15px; border-radius: 8px; border: 1px solid #ffe49c;'>{model.Message}</p>
                        </div>
                        <hr style='border: 1px solid #ffd76b; margin-top: 25px;' />
                        <p style='text-align:center; font-size: 13px; color: #888;'>This message was sent from the Wrap 'n Roll website contact form.</p>
                    </div>";

                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Wrap 'n Roll Website"),
                    Subject = $"📩 {model.Subject}",
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(receiverEmail);

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    await smtp.SendMailAsync(mail);
                }

                // 🧡 Auto-confirmation email to the customer
                string confirmSubject = "Wrap 'n Roll — Thanks for reaching out!";
                string confirmBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h3 style='color: #dba400;'>Hi {model.Name},</h3>
                        <p>Thank you for contacting <strong>Wrap 'n Roll</strong>! We’ve received your message:</p>
                        <blockquote style='border-left: 4px solid #ffd76b; padding-left: 10px; color: #555;'>{model.Message}</blockquote>
                        <p>We’ll get back to you soon. 🍔</p>
                        <br>
                        <p style='color: #888; font-size: 12px;'>This is an automated response, please don’t reply directly to this email.</p>
                    </div>";

                var confirmMail = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Wrap 'n Roll"),
                    Subject = confirmSubject,
                    Body = confirmBody,
                    IsBodyHtml = true
                };
                confirmMail.To.Add(model.Email);

                using (var smtpConfirm = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpConfirm.EnableSsl = true;
                    smtpConfirm.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    await smtpConfirm.SendMailAsync(confirmMail);
                }

                TempData["Success"] = "Your message has been sent successfully! We’ll get back to you soon 🍔";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Email failed: {ex.Message}";
                return View(model);
            }
        }
    }
}
