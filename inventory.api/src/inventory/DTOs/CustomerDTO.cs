using System.ComponentModel.DataAnnotations;

namespace inventory.DTOs
{
    public class CreateCustomerDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CustomerType { get; set; } = "Retail"; // Retail or Wholesale

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CreditLimit { get; set; }
    }

    public class UpdateCustomerDTO : CreateCustomerDTO
    {
        [Required]
        public int CustomerId { get; set; }
    }

    public class UpdateCustomerCreditDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal NewCreditLimit { get; set; }

        public string? Reason { get; set; }
    }
}