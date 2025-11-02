using System.ComponentModel.DataAnnotations;

namespace Wrap_nRoll.Models
{
    public class ContactFormModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [Display(Name = "Your Name")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Your Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a subject.")]
        [StringLength(100, ErrorMessage = "Subject cannot be longer than 100 characters.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter your message.")]
        [Display(Name = "Message")]
        [StringLength(1000, ErrorMessage = "Message cannot be longer than 1000 characters.")]
        public string Message { get; set; }
    }
}
