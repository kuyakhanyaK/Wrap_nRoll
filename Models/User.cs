using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Wrap_nRoll.Models
{
    public class User : IdentityUser<int>
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        [StringLength(200)]
        public string? Address { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
