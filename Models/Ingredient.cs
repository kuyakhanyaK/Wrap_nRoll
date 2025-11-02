using System.ComponentModel.DataAnnotations;

namespace Wrap_nRoll.Models
{
    public class Ingredient
    {
        [Key]
        public int IngredientId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        // Optional: list of wraps that include this ingredient
        public ICollection<Wrap>? Wraps { get; set; }
    }
}
