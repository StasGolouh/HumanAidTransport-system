using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CarrierRating
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CarrierId { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; } 

    // Навігаційна властивість для перевізника
    [ForeignKey("CarrierId")]
    public Carrier Carrier { get; set; }
}