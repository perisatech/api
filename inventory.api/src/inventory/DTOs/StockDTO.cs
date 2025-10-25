using System.ComponentModel.DataAnnotations;

namespace inventory.DTOs
{
    public class CreateStockDTO
    {
        [Required]
        [StringLength(100)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Category { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal WholesalePrice { get; set; }

        [Required]
        public int CurrentQuantity { get; set; }

        public int MinimumQuantity { get; set; }
        public int ReorderPoint { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }

        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? UnitOfMeasure { get; set; }
    }

    public class UpdateStockDTO : CreateStockDTO
    {
        [Required]
        public int StockId { get; set; }
    }

    public class StockQuantityUpdateDTO
    {
        [Required]
        public int StockId { get; set; }

        [Required]
        [Range(-10000, 10000)]
        public int QuantityChange { get; set; }

        public string? Reason { get; set; }
    }
}