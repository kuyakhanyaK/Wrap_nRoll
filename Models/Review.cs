using System;
using System.ComponentModel.DataAnnotations;

namespace Wrap_nRoll.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int WrapId { get; set; }
        public Wrap Wrap { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public User Customer { get; set; }

        [StringLength(300, ErrorMessage = "Comment cannot exceed 300 characters.")]
        public string Comment { get; set; }

        [Required, Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Posted")]
        public DateTime DatePosted { get; set; } = DateTime.Now;
        public string? ImageUrl { get; set; }
    }
}
