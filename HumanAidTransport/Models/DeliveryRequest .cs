using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanAidTransport.Models
{
    public class DeliveryRequest
    {
        [Key]
        public int DeliveryRequestId { get; set; }

        [Required]
        public double? Rating { get; set; } = 0;

        [ForeignKey("CarrierId")]
        public int CarrierId { get; set; } 

        public Carrier? Carrier { get; set; }

    }
}
