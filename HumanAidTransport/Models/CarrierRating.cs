using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CarrierRating
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("CarrierId")]
    public int CarrierId { get; set; }

    public int Rating { get; set; } 

    public Carrier Carrier { get; set; }

    public int NotificationId { get; set; }
}