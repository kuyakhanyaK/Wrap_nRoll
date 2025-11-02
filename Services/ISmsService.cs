using System.Threading.Tasks;

namespace Wrap_nRoll.Services
{
    public interface ISmsService
    {
        /// <summary>
        /// Sends an SMS using an Email-to-SMS gateway.
        /// </summary>
        /// <param name="to">The phone number (digits only).</param>
        /// <param name="carrierGateway">The carrier's SMS gateway domain (e.g., mtn.co.za).</param>
        /// <param name="message">The SMS message text.</param>
        Task SendSmsAsync(string to, string carrierGateway, string message);
    }
}
