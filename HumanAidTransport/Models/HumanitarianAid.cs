using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum AidType
{
    Food,
    Medicine,
    Clothes,
    Shelter,
    Military,
    Other
}

public enum Priority
{
    Low,
    Medium,
    High
}


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

        [Required]
        public AidType Type { get; set; }

        [Required]
        public Priority PriorityLevel { get; set; }


        [ForeignKey("VolunteerId")]

        // Зв'язок з волонтером
        public int VolunteerId { get; set; }

        public string Status { get; set; } = "New";

    }
}
