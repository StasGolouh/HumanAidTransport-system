using System.ComponentModel.DataAnnotations;
using HumanAidTransport.Models;

public class Carrier
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Password { get; set; }

    [Required]
    public string? Contacts { get; set; }

    [Required]
    public string? VehicleName { get; set; }

    [Required]
    public string? VehicleModel { get; set; }

    [Required]
    public string? VehicleNumber { get; set; }

    [Required]
    public string ProfilePhotoURL { get; set; } = "/images/profile_photos/photodef.jpg";

    [Required]
    public int Capacity { get; set; } 

    [Required]
    public string? Dimensions { get; set; }

    [Required]
    public string CardNumber { get; set; }

    [Required]
    public string CVV { get; set; }

    [Required]
    public int Balance { get; set; }

    public List<HumanitarianAid> AvailableTasks { get; set; } = new List<HumanitarianAid>();

    public List<CarrierRating> Ratings { get; set; } = new List<CarrierRating>();

    public int AverageRating
    {
        get
        {
            return (Ratings != null && Ratings.Count > 0) ? (int)Math.Round(Ratings.Average(r => r.Rating)) : 1;
        }
    }
}
