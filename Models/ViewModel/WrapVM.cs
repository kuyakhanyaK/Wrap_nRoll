using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wrap_nRoll.Models.ViewModel
{
    public class WrapVM
    {
        public int WrapId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(300)]
        public string Description { get; set; }

        [Required, Range(5, 500)]
        public decimal Price { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile ImageFile { get; set; } // File uploaded by admin

        public string? ImageUrl { get; set; } // Saved path after upload

        public bool IsAvailable { get; set; } = true;

        // --- Customer customization ---
        public List<IngredientVM> Ingredients { get; set; } = new(); // All default ingredients

        [Range(1, 20, ErrorMessage = "Quantity must be between 1 and 20.")]
        public int Quantity { get; set; } = 1; // Default order quantity

        // List of ingredient names the customer has selected
        public List<string> SelectedIngredients { get; set; } = new();
    }

    public class IngredientVM
    {
        public int IngredientId { get; set; }
        public string Name { get; set; }

        // True if the ingredient is included in the wrap
        public bool Selected { get; set; } = true;
    }
}
