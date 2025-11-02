namespace Wrap_nRoll.Models
{
    using System.Collections.Generic;

    public class CartItem
    {
        // 🔹 Core wrap details
        public int WrapId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // 🔹 Image (for displaying on cart page)
        public string ImageUrl { get; set; }

        // 🔹 Fillings / custom ingredients chosen by customer
        public List<string> Ingredients { get; set; } = new List<string>();

        // 🔹 Stored as text so it can be easily serialized for session / checkout
        public string Customizations { get; set; }

        // 🔹 For subtotal calculations
        public decimal TotalPrice => Price * Quantity;
    }
}
