using System.ComponentModel.DataAnnotations;


public class Carrier
{
    [Key]
    public int CarrierId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    [RegularExpression(@"^\+?[0-9]{1,4}?[ -]?[0-9]{1,3}[ -]?[0-9]{1,4}[ -]?[0-9]{1,4}$",ErrorMessage = "Невірний формат номера телефону.")]
    public string Contacts { get; set; }

    [Required]
    public string VehicleName { get; set; } 
    [Required]
    public string VehicleModel { get; set; } 

    [Required]
    [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "Некоректний номерний знак")]
    public string VehicleNumber { get; set; }

    public double Rating { get; set; } = 0;

    public int RatingCount { get; set; } = 0;
}
