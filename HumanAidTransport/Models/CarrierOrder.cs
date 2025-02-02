using System.ComponentModel.DataAnnotations;

namespace HumanAidTransport.Models
{
    public class CarrierOrder
    {
        [Key]
        public int CarrierOrderId { get; set; }
        public int CarrierId { get; set; }
        public Carrier Carrier { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
