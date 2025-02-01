using System.ComponentModel.DataAnnotations;

namespace HumanAidTransport.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int CarrierId { get; set; }
        public Carrier Carrier { get; set; }

        [Range(1, 5)] // Оцінка між 1 і 5
        public int RatingValue { get; set; }

        public string Feedback { get; set; }
    }
}
