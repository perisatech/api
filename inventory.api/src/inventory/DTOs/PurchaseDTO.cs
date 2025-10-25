using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace inventory.DTOs
{
    public class CreatePurchaseItemDTO
    {
        [Required]
        public int StockId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class CreatePurchaseDTO
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        public List<CreatePurchaseItemDTO> Items { get; set; } = new();

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? PaymentDueDate { get; set; }
    }

    public class UpdatePurchaseStatusDTO
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // Pending, Received, Cancelled

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdatePurchasePaymentDTO
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Partial"; // Pending, Partial, Paid
    }
}