using System.ComponentModel.DataAnnotations;
using HumanAidTransport.Models;

public class Volunteer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string ProfilePhotoURL { get; set; } = "/images/profile_photos/photodef.jpg";

    [Required]
    public string CardNumber { get; set; }

    [Required]
    public string CVV { get; set; }

    [Required]
    public double Balance { get; set; }

    [Required]
    public double Debt { get; set; } = 0;

    [Required]
    public int ViolationsCount { get; set; } = 0;

    public List<HumanitarianAid> Tasks { get; set; } = new List<HumanitarianAid>();

    public List<DeliveryRequest> DeliveryRequests { get; set; } = new List<DeliveryRequest>();

}
