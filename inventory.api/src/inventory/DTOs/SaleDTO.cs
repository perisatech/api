using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace inventory.DTOs
{
    public class CreateSaleItemDTO
    {
        [Required]
        public int StockId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; }
    }

    public class CreateSaleDTO
    {
        [Required]
        public int CustomerId { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash";

        public DateTime? PaymentDueDate { get; set; }

        [Required]
        public List<CreateSaleItemDTO> Items { get; set; } = new();
    }

    public class UpdateSaleStatusDTO
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateSalePaymentDTO
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}