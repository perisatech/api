using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inventory.Models
{
    public class Sale
    {
        [Key]
        public int SaleId { get; set; }

        public int CustomerId { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Partial, Paid
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Credit
        public DateTime? PaymentDueDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }

    public class SaleItem
    {
        [Key]
        public int SaleItemId { get; set; }

        public int SaleId { get; set; }
        public int StockId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        // Navigation properties
        public virtual Sale Sale { get; set; } = null!;
        public virtual Stock Stock { get; set; } = null!;
    }
}