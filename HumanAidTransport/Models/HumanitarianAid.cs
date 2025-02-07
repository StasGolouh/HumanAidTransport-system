using System;
using System.ComponentModel.DataAnnotations;

namespace HumanAidTransport.Models
{
    public class HumanitarianAid
    {
        [Key]
        public int HumanAidId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
        public string Name { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        [StringLength(500, ErrorMessage = "Description must be at most 500 characters.")]
        public string? Description { get; set; }

        [Required]

        [Range(0, double.MaxValue, ErrorMessage = "Payment must be a positive number.")]
        public double? Payment { get; set; }
        
        public string PaymentStatus => Payment.HasValue ? Payment.Value.ToString("F2") : "Free";

        [FutureDate(ErrorMessage = "Expected delivery time must be in the future.")]
        public DateTime? ExpectedDeliveryTime { get; set; }

        public DateTime? ActualDeliveryTime { get; set; }

        [Required]
        public string DeliveryAddressFrom { get; set; }

        [Required]
        public string DeliveryAddressTo { get; set; }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime > DateTime.Now; // Дата має бути у майбутньому
            }
            return true; 
        }
    }
}
