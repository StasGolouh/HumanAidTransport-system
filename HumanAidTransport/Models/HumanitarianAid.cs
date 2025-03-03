using System.ComponentModel.DataAnnotations;

namespace HumanAidTransport.Models
{
    public class HumanitarianAid
    {
        [Key]
        public int HumanAidId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        public string? Description { get; set; }

        [Required]
        public double? Payment { get; set; }

        [Required]
        public DateTime? ExpectedDeliveryTime { get; set; }

        [Required]
        public string DeliveryAddressFrom { get; set; }

        [Required]
        public string DeliveryAddressTo { get; set; }

        // Зв'язок з волонтером
        public int VolunteerId { get; set; }

        public string Status { get; set; } = "Pending";

    }
}
