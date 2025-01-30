using System.ComponentModel.DataAnnotations;

namespace HumanAidTransport.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [Required]
        public int HumanAidId { get; set; }
        public HumanitarianAid HumanitarianAid { get; set; }
        [Required]
        public int CarrierId { get; set; }
        public Carrier Carrier { get; set; }
        [Required]
        public string DestinationAddress { get; set; }
        [Required]
        public decimal Payment { get; set; }
        [Required]
        public DateTime ExpectedDeliveryTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
        public decimal Fine { get; set; } = 0;
        public string Status { get; set; }
        public double? Rating { get; set; }
    }
}
