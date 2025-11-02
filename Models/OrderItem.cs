using System.ComponentModel.DataAnnotations;

namespace Wrap_nRoll.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        public int WrapId { get; set; }
        public Wrap Wrap { get; set; }

        [Required, Range(1, 20, ErrorMessage = "Quantity must be between 1 and 20.")]
        public int Quantity { get; set; }

        [Required, Range(1, 500, ErrorMessage = "Price must be valid.")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        // <-- Add this property
        [StringLength(500)]
        public string? Customizations { get; set; }

    }

}
