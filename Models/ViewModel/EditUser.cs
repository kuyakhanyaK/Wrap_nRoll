using System.ComponentModel.DataAnnotations;

namespace Wrap_nRoll.Models.ViewModel
{
    public class EditUserVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
