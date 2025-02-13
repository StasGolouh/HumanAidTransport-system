using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    [RegularExpression(@"^\+?[0-9]{1,4}?[ -]?[0-9]{1,3}[ -]?[0-9]{1,4}[ -]?[0-9]{1,4}$", ErrorMessage = "The phone number format is incorrect.")]
    public string? Contacts { get; set; }

    [Required]
    public string? VehicleName { get; set; }

    [Required]
    public string? VehicleModel { get; set; }

    [Required]
    [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "Incorrect number plate")]
    public string? VehicleNumber { get; set; }

    [Required]
    public string ProfilePhotoURL { get; set; } = "/images/profile_photos/photodef.jpg";

    public List<HumanitarianAid> AvailableTasks { get; set; } = new List<HumanitarianAid>();

    // ⭐ Додаємо список оцінок (один-to-багато)
    public List<CarrierRating> Ratings { get; set; } = new List<CarrierRating>();

    [Required]
    public int AverageRating => Ratings.Any() ? (int)Math.Round(Ratings.Average(r => r.Rating)): 1;
}
