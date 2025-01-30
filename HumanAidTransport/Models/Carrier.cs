using System.ComponentModel.DataAnnotations;

namespace HumanAidTransport.Models
{
    public class Carrier
    {
        [Key]
        public int CarrierId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ContactInfo { get; set; }
        public ICollection<Order> Orders { get; set; }
        public double Rating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
    }
}
