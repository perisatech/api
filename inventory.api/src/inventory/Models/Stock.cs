using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inventory.Models
{
    public class Stock
    {
        [Key]
        public int StockId { get; set; }

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

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WholesalePrice { get; set; }

        public int CurrentQuantity { get; set; }
        public int MinimumQuantity { get; set; }
        public int ReorderPoint { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }

        // Additional fields for different business types
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? UnitOfMeasure { get; set; }
    }
}