using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inventory.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }

        public int ClientId { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string PurchaseNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Received, Cancelled
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Partial, Paid
        public DateTime? PaymentDueDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }

    public class PurchaseItem
    {
        [Key]
        public int PurchaseItemId { get; set; }

        public int PurchaseId { get; set; }
        public int StockId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Navigation properties
        public virtual Purchase Purchase { get; set; } = null!;
        public virtual Stock Stock { get; set; } = null!;
    }
}