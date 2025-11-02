using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wrap_nRoll.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public User Customer { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required, Range(0, double.MaxValue, ErrorMessage = "Total amount must be valid.")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Preparing, Ready, Completed, Cancelled

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        
    }
}
