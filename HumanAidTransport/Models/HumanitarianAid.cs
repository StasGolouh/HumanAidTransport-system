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
        public int Quantity { get; set; }
        public string Description { get; set; }
    }
}
