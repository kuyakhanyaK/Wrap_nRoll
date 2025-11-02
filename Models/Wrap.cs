using System.ComponentModel.DataAnnotations;
using Wrap_nRoll.Models.ViewModel;

namespace Wrap_nRoll.Models
{
    public class Wrap
    {
        [Key]
        public int WrapId { get; set; }

        [Required, StringLength(100, ErrorMessage = "Wrap name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string Description { get; set; }

        [Required, Range(5, 500, ErrorMessage = "Price must be between R5 and R500.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(200)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Available?")]
        public bool IsAvailable { get; set; } = true;
        public ICollection<Ingredient>? Ingredients { get; set; } = new List<Ingredient>();
    }
}
